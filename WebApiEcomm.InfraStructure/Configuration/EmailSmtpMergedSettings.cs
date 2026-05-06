using MailKit.Security;
using Microsoft.Extensions.Configuration;

namespace WebApiEcomm.InfraStructure.Configuration
{
    /// <summary>
    /// SMTP settings merged from <c>Email:*</c> and legacy <c>EmailSetting:*</c>.
    /// </summary>
    public sealed class EmailSmtpMergedSettings
    {
        public string FromName { get; private init; } = "WebApiEcomm";
        public string FromAddress { get; private init; } = string.Empty;
        public string Host { get; private init; } = string.Empty;
        public int Port { get; private init; }
        public string UserName { get; private init; } = string.Empty;
        public string Password { get; private init; } = string.Empty;
        public bool StartTlsPreferSslConnect { get; private init; }

        public static EmailSmtpMergedSettings FromConfiguration(IConfiguration configuration)
        {
            var email = configuration.GetSection("Email");
            var legacy = configuration.GetSection("EmailSetting");

            var fromAddr = email["FromAddress"]
                         ?? legacy["From"]
                         ?? string.Empty;

            var fromName = email["FromName"]
                           ?? legacy["FromName"]
                           ?? "WebApiEcomm";

            var host = email["SmtpHost"]
                       ?? legacy["Smtp"]
                       ?? string.Empty;

            var legacyPortOk = TryParsePort(legacy["Port"], out var legacyPort);
            var emailPortOk = TryParsePort(email["Port"], out var emailPort);
            var port = emailPortOk ? emailPort : (legacyPortOk ? legacyPort : 587);

            var userName = email["UserName"] ?? legacy["UserName"] ?? string.Empty;
            var password = email["Password"] ?? legacy["Password"] ?? string.Empty;

            var legacyUseSslParse = legacy["UseSsl"] is { } lu && bool.TryParse(lu, out var lssl) ? lssl : (bool?)null;
            var emailUseSslParse = email["UseSsl"] is { } eu && bool.TryParse(eu, out var essl) ? essl : (bool?)null;
            var useSsl = legacyUseSslParse ?? emailUseSslParse ?? false;

            return new EmailSmtpMergedSettings
            {
                FromName = string.IsNullOrWhiteSpace(fromName) ? "WebApiEcomm" : fromName,
                FromAddress = fromAddr,
                Host = host,
                Port = port,
                UserName = userName,
                Password = password,
                StartTlsPreferSslConnect = useSsl
            };
        }

        public SecureSocketOptions ResolveSecureSocketOption()
            => StartTlsPreferSslConnect ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

        private static bool TryParsePort(string? raw, out int port)
        {
            port = 0;
            return !string.IsNullOrWhiteSpace(raw) && int.TryParse(raw, out port) && port > 0;
        }
    }
}
