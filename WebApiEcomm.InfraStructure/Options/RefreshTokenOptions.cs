namespace WebApiEcomm.InfraStructure.Options
{
    public class RefreshTokenOptions
    {
        public const string SectionName = "RefreshToken";
        public string SecretPepper { get; set; } = string.Empty;
        public int ExpiryDays { get; set; } = 30;
        public int AbsoluteMaxLifetimeDays { get; set; } = 60;
    }
}
