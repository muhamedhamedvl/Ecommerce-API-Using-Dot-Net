using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApiEcomm.Core.Interfaces;
using WebApiEcomm.Core.Interfaces.Auth;
using WebApiEcomm.Core.Interfaces.IUnitOfWork;
using WebApiEcomm.Core.Services;
using WebApiEcomm.InfraStructure.Configuration;
using WebApiEcomm.InfraStructure.Data;
using WebApiEcomm.InfraStructure.Repositores;
using WebApiEcomm.InfraStructure.Repositores.Service;
using WebApiEcomm.InfraStructure.Repositores.UnitOfWork;
using WebApiEcomm.InfraStructure.Repositories;

namespace WebApiEcomm.InfraStructure
{
    public static class InfraStructureRegisteration
    {
        public static IServiceCollection InfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
            services.AddSingleton(_ => EmailSmtpMergedSettings.FromConfiguration(configuration));
            services.AddSingleton(_ => StripeMergedSettings.FromConfiguration(configuration));

            services.AddScoped(typeof(GenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuth, AuthRepository>();
            services.AddScoped<IImageManagementService, ImageManagementService>();
            services.AddScoped<IGenrateToken, GenrateToken>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IOrderService, OrderService>();

            // Redis / basket repository: configured from Program.cs (Redis:ConnectionString + environment rules).

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            });

            return services;
        }
    }
}
