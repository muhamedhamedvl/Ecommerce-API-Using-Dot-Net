using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using WebApiEcomm.API.Middleware;
using WebApiEcomm.Core.Entites.Identity;
using WebApiEcomm.Core.Interfaces;
using WebApiEcomm.InfraStructure;
using WebApiEcomm.InfraStructure.Configuration;
using WebApiEcomm.InfraStructure.Data;
using WebApiEcomm.InfraStructure.Repositores;

namespace WebApiEcomm.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddControllers();

            var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? ["https://localhost:4200"];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CORSPolicy", policy =>
                {
                    policy
                        .WithOrigins(corsOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            builder.Services.AddOpenApi();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "E-Commerce API",
                    Version = "v1",
                    Description = "E-Commerce API"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddDataProtection();

            builder.Services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddSignInManager<SignInManager<AppUser>>()
                .AddDefaultTokenProviders();

            using var jwtBootstrapLoggerFactory = LoggerFactory.Create(lb =>
            {
                lb.AddConfiguration(builder.Configuration.GetSection("Logging"));
                lb.AddConsole();
                lb.AddDebug();
            });
            var jwtStartupLogger = jwtBootstrapLoggerFactory.CreateLogger<Program>();

            var (resolvedSecretRaw, resolvedSecretSource) = JwtSecretResolver.Resolve(builder.Configuration);

            string signingSecret;
            if (!string.IsNullOrWhiteSpace(resolvedSecretRaw))
            {
                signingSecret = resolvedSecretRaw.Trim();
                jwtStartupLogger.LogInformation(
                    "JWT signing key loaded from {JwtSecretSource}",
                    resolvedSecretSource);
            }
            else if (builder.Environment.IsProduction())
            {
                throw new InvalidOperationException(
                    "JWT signing key is not configured for the Production environment. " +
                    "Set a strong secret using environment variables JWT__SECRET or Jwt__Secret (recommended on servers), " +
                    "or configuration keys Jwt:Secret / Token:Secret (legacy) via a secure provider " +
                    "(e.g. hosting panel, Key Vault, user secrets are not for production). " +
                    "Do not commit secrets to source control.");
            }
            else
            {
                signingSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                jwtStartupLogger.LogWarning(
                    "No JWT signing key was found in configuration or environment (checked JWT__SECRET, Jwt__Secret, TOKEN__SECRET, Token__Secret, Jwt:Secret, Token:Secret). " +
                    "Environment: {EnvironmentName}. Using an auto-generated ephemeral development key; all issued JWTs become invalid when the application restarts. " +
                    "For stable local development set Jwt__Secret, user secrets, or appsettings.Development.json (never commit real secrets).",
                    builder.Environment.EnvironmentName);
            }

            var jwtMerged = JwtMergedSettings.FromConfiguration(builder.Configuration, signingSecret);

            builder.Services.AddSingleton(jwtMerged);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtMerged.Secret)),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(jwtMerged.Issuer),
                    ValidIssuer = jwtMerged.Issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(jwtMerged.Audience),
                    ValidAudience = jwtMerged.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            var permitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 100);
            var windowSeconds = builder.Configuration.GetValue("RateLimiting:WindowSeconds", 60);

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? httpContext.Request.Headers.Host.ToString()
                        ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            Window = TimeSpan.FromSeconds(windowSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });
            });

            ConfigureBasketStorage(builder.Services, builder.Configuration, builder.Environment);

            builder.Services.InfrastructureConfiguration(builder.Configuration);
            builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

            var app = builder.Build();

            await app.InitializeApplicationAsync().ConfigureAwait(false);

            app.UseForwardedHeaders();

            app.UseSerilogRequestLogging();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API V1");
                c.RoutePrefix = ""; 
            });

            // IIS / reverse proxies: forward X-Forwarded-Proto first (above). Disable redirection if terminating TLS upstream.
            if (!app.Configuration.GetValue("Hosting:DisableHttpsRedirection", false))
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("CORSPolicy");

            app.UseRateLimiter();

            app.UseMiddleware<ExceptionsMiddleware>();
            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        /// <summary>
        /// Redis-backed basket in production; optional in non-production with in-memory fallback.
        /// </summary>
        private static void ConfigureBasketStorage(
            IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            var redisConnection =
                configuration["Redis:ConnectionString"]?.Trim();

            if (string.IsNullOrWhiteSpace(redisConnection))
            {
                redisConnection = configuration.GetConnectionString("Redis")
                    ?? configuration.GetConnectionString("redis");
            }

            if (!string.IsNullOrWhiteSpace(redisConnection))
            {
                services.AddSingleton<IConnectionMultiplexer>(_ =>
                    ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(redisConnection)));

                services.AddScoped<ICustomerBasketRepository, CustomerBasketRepository>();
                return;
            }

            if (environment.IsProduction())
            {
                throw new InvalidOperationException(
                    "Redis is required for basket storage in Production. " +
                    "Set configuration key Redis:ConnectionString to a valid StackExchange.Redis connection string (or use ConnectionStrings:Redis / ConnectionStrings:redis for legacy hosting layouts). " +
                    "Do not leave Redis optional in Production.");
            }

            services.AddSingleton<ICustomerBasketRepository, InMemoryBasketRepository>();
        }
    }

    internal static class ApplicationInitializationExtensions
    {
        internal static async Task InitializeApplicationAsync(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
                return;

            await using var scope = app.Services.CreateAsyncScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                var userManager = services.GetRequiredService<UserManager<AppUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var logger = services.GetRequiredService<ILogger<Program>>();

                await AppDbContextSeed.SeedAsync(context, userManager, roleManager, logger, applyMigrations: true)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating or seeding the database (Development only)");
            }
        }
    }
}
