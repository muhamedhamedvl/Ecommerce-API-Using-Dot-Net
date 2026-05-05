using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApiEcomm.InfraStructure.Entities.Auth;

namespace WebApiEcomm.InfraStructure.Data.Config
{
    public class EmailVerificationEntityConfiguration : IEntityTypeConfiguration<EmailVerificationEntity>
    {
        public void Configure(EntityTypeBuilder<EmailVerificationEntity> builder)
        {
            builder.ToTable("EmailVerificationCodes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            builder.Property(x => x.CodeHash).HasMaxLength(128).IsRequired();
            builder.HasIndex(x => new { x.UserId, x.IsUsed, x.ExpiresAtUtc });
        }
    }
}
