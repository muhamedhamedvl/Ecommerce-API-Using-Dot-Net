using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApiEcomm.InfraStructure.Entities.Auth;

namespace WebApiEcomm.InfraStructure.Data.Config
{
    public class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
    {
        public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            builder.Property(x => x.JwtId).HasMaxLength(128).IsRequired();
            builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            builder.Property(x => x.CreatedByIp).HasMaxLength(64);
            builder.Property(x => x.UserAgent).HasMaxLength(256);
            builder.Property(x => x.RevokedReason).HasMaxLength(256);
            builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(128);

            builder.HasIndex(x => new { x.UserId, x.TokenHash }).IsUnique();
            builder.HasIndex(x => x.JwtId);
            builder.HasIndex(x => new { x.UserId, x.IsRevoked, x.ExpiresAtUtc });
        }
    }
}
