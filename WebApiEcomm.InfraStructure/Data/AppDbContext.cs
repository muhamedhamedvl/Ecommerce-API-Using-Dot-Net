using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiEcomm.Core.Entites.Identity;
using WebApiEcomm.Core.Entites.Order;
using WebApiEcomm.Core.Entites.Product;
using WebApiEcomm.InfraStructure.Entities.Auth;
namespace WebApiEcomm.InfraStructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Photo> Photos { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderItem> OrderItems { get; set; }
        public virtual DbSet<DeliveryMethod> DeliveryMethods { get; set; }
        public virtual DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
        public virtual DbSet<EmailVerificationEntity> EmailVerificationCodes { get; set; }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 


            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.UserAddress)
                .WithOne(a => a.AppUser)
                .HasForeignKey<Address>(a => a.AppUserId);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
