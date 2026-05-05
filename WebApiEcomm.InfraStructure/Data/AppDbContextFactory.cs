using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WebApiEcomm.InfraStructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "WebApiEcomm.API");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connection = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Database=EcommerceDb;Trusted_Connection=True;TrustServerCertificate=True;";
            optionsBuilder.UseSqlServer(connection);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
