using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApiEcomm.Core.Entites.Identity;

namespace WebApiEcomm.InfraStructure.Data
{
    public class AppDbContextSeed
    {
        public static async Task SeedAsync(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            try
            {
                // Ensure database is created
                await context.Database.MigrateAsync();

                // Seed Roles
                if (!await roleManager.Roles.AnyAsync())
                {
                    var roles = new List<IdentityRole>
                    {
                        new IdentityRole { Name = AppRoles.Admin, NormalizedName = AppRoles.Admin.ToUpper() },
                        new IdentityRole { Name = AppRoles.User, NormalizedName = AppRoles.User.ToUpper() }
                    };

                    foreach (var role in roles)
                    {
                        await roleManager.CreateAsync(role);
                    }

                    logger.LogInformation("Roles seeded successfully");
                }

                // Seed Admin User
                if (!await userManager.Users.AnyAsync(u => u.Email == "admin@ecommerce.com"))
                {
                    var adminUser = new AppUser
                    {
                        UserName = "admin",
                        Email = "admin@ecommerce.com",
                        DisplayName = "System Administrator",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
                        logger.LogInformation("Admin user created successfully");
                        logger.LogInformation("Admin credentials - Email: admin@ecommerce.com, Password: Admin@123");
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
            }
        }
    }
}
