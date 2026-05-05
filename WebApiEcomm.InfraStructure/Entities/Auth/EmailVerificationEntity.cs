namespace WebApiEcomm.InfraStructure.Entities.Auth
{
    public class EmailVerificationEntity
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string CodeHash { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public int AttemptCount { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
