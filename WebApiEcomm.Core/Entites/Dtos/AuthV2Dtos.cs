using System.ComponentModel.DataAnnotations;

namespace WebApiEcomm.Core.Entites.Dtos
{
    public record RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        [MinLength(3)]
        public string UserName { get; init; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; init; } = string.Empty;
    }

    public record LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        public string Password { get; init; } = string.Empty;
    }

    public record RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; init; } = string.Empty;
    }

    public record VerifyEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(6)]
        public string Code { get; init; } = string.Empty;
    }

    public record ResendVerificationRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;
    }

    public record LogoutRequest
    {
        [Required]
        public string RefreshToken { get; init; } = string.Empty;
    }

    public record TokenPairResponse
    {
        public string AccessToken { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
        public long ExpiresInSeconds { get; init; }
        public string TokenType { get; init; } = "Bearer";
        public IList<string> Roles { get; init; } = new List<string>();
        public string Email { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
    }
}
