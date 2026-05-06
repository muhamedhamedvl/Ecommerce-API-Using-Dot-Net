namespace WebApiEcomm.InfraStructure.Configuration
{
    public sealed class EmailSettings
    {
        public const string SectionName = "EmailSettings";

        public string FromName { get; set; } = "WebApiEcomm";
        public string From { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSSL { get; set; } = true;
    }
}
