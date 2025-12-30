using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiEcomm.Core.Interfaces.IUnitOfWork;
using WebApiEcomm.Core.Interfaces;
using WebApiEcomm.Core.Interfaces.Auth;
using WebApiEcomm.InfraStructure.Data;
using Microsoft.EntityFrameworkCore;
using WebApiEcomm.InfraStructure.Repositores.UnitOfWork;
using WebApiEcomm.InfraStructure.Repositories;
using WebApiEcomm.Core.Services;
using WebApiEcomm.InfraStructure.Repositores.Service;
using WebApiEcomm.InfraStructure.Repositores;
using Microsoft.Extensions.FileProviders;
using StackExchange.Redis;

namespace WebApiEcomm.InfraStructure
{
    public static class InfraStructureRegisteration
    {
        public static IServiceCollection InfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(GenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuth, AuthRepository>();
            services.AddSingleton<IImageManagementService, ImageManagementService>();
            services.AddScoped<IGenrateToken, GenrateToken>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
            //Apply redis
            services.AddSingleton<IConnectionMultiplexer>(i =>
            {
                var config = ConfigurationOptions.Parse(configuration.GetConnectionString("redis"));
                return ConnectionMultiplexer.Connect(config);
            });
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
            return services;
        }
    }
}
