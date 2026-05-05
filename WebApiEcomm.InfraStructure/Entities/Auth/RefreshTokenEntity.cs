namespace WebApiEcomm.InfraStructure.Entities.Auth
{
    public class RefreshTokenEntity
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string TokenHash { get; set; } = string.Empty;
        public string JwtId { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public string? ReplacedByTokenHash { get; set; }
        public string? CreatedByIp { get; set; }
        public string? UserAgent { get; set; }
        public bool IsRevoked { get; set; }
        public string? RevokedReason { get; set; }
    }
}
