using WebApiEcomm.Core.Entites.Auth;

namespace WebApiEcomm.Core.Services.Auth
{
    public interface IRefreshTokenStore
    {
        Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task RevokeAsync(RefreshToken token, string reason, CancellationToken cancellationToken = default);
        Task RevokeFamilyAsync(string userId, string jwtId, string reason, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
