using MailKit.Security;
using Microsoft.Extensions.Configuration;

namespace WebApiEcomm.InfraStructure.Configuration
{
    /// <summary>
    /// SMTP settings merged from <c>Email:*</c>, <c>EmailSettings:*</c>, and legacy <c>EmailSetting:*</c>.
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
            var emailSettings = configuration.GetSection("EmailSettings");
            var legacy = configuration.GetSection("EmailSetting");

            var fromAddr = email["FromAddress"]
                         ?? emailSettings["From"]
                         ?? legacy["From"]
                         ?? string.Empty;

            var fromName = email["FromName"]
                           ?? emailSettings["FromName"]
                           ?? legacy["FromName"]
                           ?? "WebApiEcomm";

            var host = email["SmtpHost"]
                       ?? emailSettings["Host"]
                       ?? legacy["Smtp"]
                       ?? string.Empty;

            var emailSettingsPortOk = TryParsePort(emailSettings["Port"], out var emailSettingsPort);
            var legacyPortOk = TryParsePort(legacy["Port"], out var legacyPort);
            var emailPortOk = TryParsePort(email["Port"], out var emailPort);
            var port = emailPortOk
                ? emailPort
                : (emailSettingsPortOk ? emailSettingsPort : (legacyPortOk ? legacyPort : 587));

            var userName = email["UserName"] ?? emailSettings["UserName"] ?? legacy["UserName"] ?? string.Empty;
            var password = email["Password"] ?? emailSettings["Password"] ?? legacy["Password"] ?? string.Empty;

            var legacyUseSslParse = legacy["UseSsl"] is { } lu && bool.TryParse(lu, out var lssl) ? lssl : (bool?)null;
            var emailUseSslParse = email["UseSsl"] is { } eu && bool.TryParse(eu, out var essl) ? essl : (bool?)null;
            var emailSettingsUseSslParse =
                emailSettings["EnableSSL"] is { } es && bool.TryParse(es, out var esssl)
                    ? esssl
                    : (bool?)null;
            var useSsl = emailUseSslParse ?? emailSettingsUseSslParse ?? legacyUseSslParse ?? false;

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
