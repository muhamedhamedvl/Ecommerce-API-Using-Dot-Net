namespace WebApiEcomm.Core.Services.Auth
{
    public interface IEmailVerificationService
    {
        Task<string> GenerateCodeAsync(string userId, CancellationToken cancellationToken = default);
        Task<bool> VerifyCodeAsync(string userId, string code, CancellationToken cancellationToken = default);
    }
}
