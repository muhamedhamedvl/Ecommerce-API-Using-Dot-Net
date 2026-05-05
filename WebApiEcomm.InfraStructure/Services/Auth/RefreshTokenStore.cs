using Microsoft.EntityFrameworkCore;
using WebApiEcomm.Core.Entites.Auth;
using WebApiEcomm.Core.Services.Auth;
using WebApiEcomm.InfraStructure.Data;
using WebApiEcomm.InfraStructure.Entities.Auth;

namespace WebApiEcomm.InfraStructure.Services.Auth
{
    public class RefreshTokenStore : IRefreshTokenStore
    {
        private readonly AppDbContext _context;

        public RefreshTokenStore(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            await _context.RefreshTokens.AddAsync(Map(token), cancellationToken);
        }

        public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            var entity = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
            return entity is null ? null : Map(entity);
        }

        public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return entity is null ? null : Map(entity);
        }

        public async Task RevokeAsync(RefreshToken token, string reason, CancellationToken cancellationToken = default)
        {
            var entity = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Id == token.Id, cancellationToken);
            if (entity is null) return;
            entity.IsRevoked = true;
            entity.RevokedAtUtc = DateTime.UtcNow;
            entity.RevokedReason = reason;
        }

        public async Task RevokeFamilyAsync(string userId, string jwtId, string reason, CancellationToken cancellationToken = default)
        {
            var list = await _context.RefreshTokens
                .Where(x => x.UserId == userId && x.JwtId == jwtId && !x.IsRevoked)
                .ToListAsync(cancellationToken);
            foreach (var token in list)
            {
                token.IsRevoked = true;
                token.RevokedAtUtc = DateTime.UtcNow;
                token.RevokedReason = reason;
            }
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => _context.SaveChangesAsync(cancellationToken);

        private static RefreshTokenEntity Map(RefreshToken token) => new()
        {
            Id = token.Id,
            UserId = token.UserId,
            TokenHash = token.TokenHash,
            JwtId = token.JwtId,
            CreatedAtUtc = token.CreatedAtUtc,
            ExpiresAtUtc = token.ExpiresAtUtc,
            RevokedAtUtc = token.RevokedAtUtc,
            ReplacedByTokenHash = token.ReplacedByTokenHash,
            CreatedByIp = token.CreatedByIp,
            UserAgent = token.UserAgent,
            IsRevoked = token.IsRevoked,
            RevokedReason = token.RevokedReason
        };

        private static RefreshToken Map(RefreshTokenEntity entity) => new()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            TokenHash = entity.TokenHash,
            JwtId = entity.JwtId,
            CreatedAtUtc = entity.CreatedAtUtc,
            ExpiresAtUtc = entity.ExpiresAtUtc,
            RevokedAtUtc = entity.RevokedAtUtc,
            ReplacedByTokenHash = entity.ReplacedByTokenHash,
            CreatedByIp = entity.CreatedByIp,
            UserAgent = entity.UserAgent,
            IsRevoked = entity.IsRevoked,
            RevokedReason = entity.RevokedReason
        };
    }
}
