using WebApiEcomm.Core.Entites.Dtos;

namespace WebApiEcomm.Core.Services.Auth
{
    public interface IAuthApplicationService
    {
        Task RegisterAsync(RegisterRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
        Task<TokenPairResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
        Task<TokenPairResponse> RefreshAsync(RefreshTokenRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
        Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);
        Task ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken = default);
        Task LogoutAsync(string userId, LogoutRequest request, CancellationToken cancellationToken = default);
        Task<TokenPairResponse> GetCurrentAsync(string userId, CancellationToken cancellationToken = default);
    }
}
