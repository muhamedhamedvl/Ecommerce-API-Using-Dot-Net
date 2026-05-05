using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApiEcomm.Core.Entites.Order;

namespace WebApiEcomm.InfraStructure.Data.Config
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.OwnsOne(x => x.shippingAddress, n =>
            {
                n.WithOwner();
            });
            builder.HasMany(x => x.orderItems)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.SubTotal)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.status)
                .HasConversion(
                    o => o.ToString(),
                    o => (Status)Enum.Parse(typeof(Status), o)
                );
        }
    }
}
