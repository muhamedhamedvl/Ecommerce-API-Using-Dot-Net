using MailKit.Security;

namespace WebApiEcomm.InfraStructure.Options
{
    public class EmailOptions
    {
        public const string SectionName = "Email";
        public string FromName { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string SmtpHost { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public SecureSocketOptions SecureSocketOption { get; set; } = SecureSocketOptions.StartTls;
        public int TimeoutSeconds { get; set; } = 30;

        // Backward-compatible fallback for older appsettings files.
        public bool UseSsl { get; set; } = true;

        public SecureSocketOptions GetSecureSocketOption()
        {
            if (SecureSocketOption != SecureSocketOptions.Auto)
            {
                return SecureSocketOption;
            }

            return Port == 587
                ? SecureSocketOptions.StartTls
                : UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto;
        }
    }
}
