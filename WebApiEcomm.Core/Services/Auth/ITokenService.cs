using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Identity;

namespace WebApiEcomm.Core.Services.Auth
{
    public interface ITokenService
    {
        Task<TokenPairResponse> CreateTokenPairAsync(AppUser user, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
        Task<TokenPairResponse> RotateRefreshTokenAsync(string refreshToken, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
        Task RevokeRefreshTokenAsync(string userId, string refreshToken, string reason, CancellationToken cancellationToken = default);
        string HashRefreshToken(string token);
    }
}
