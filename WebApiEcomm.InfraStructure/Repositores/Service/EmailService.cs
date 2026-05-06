using Microsoft.Extensions.Logging;
using MimeKit;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Services;
using WebApiEcomm.InfraStructure.Configuration;

namespace WebApiEcomm.InfraStructure.Repositores.Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailSmtpMergedSettings _smtp;
        private readonly ILogger<EmailService> _logger;

        public EmailService(EmailSmtpMergedSettings smtp, ILogger<EmailService> logger)
        {
            _smtp = smtp;
            _logger = logger;
        }

        public async Task SendEmail(EmailDto emailDto)
        {
            if (string.IsNullOrWhiteSpace(_smtp.Host))
            {
                _logger.LogWarning("SMTP host not configured; skipping email send to {To}", emailDto.To);
                return;
            }

            using var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_smtp.FromName, string.IsNullOrWhiteSpace(emailDto.From) ? _smtp.FromAddress : emailDto.From));
            mimeMessage.To.Add(new MailboxAddress(emailDto.To, emailDto.To));
            mimeMessage.Subject = emailDto.Subject;
            mimeMessage.Body = new TextPart("html") { Text = emailDto.Content };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                var socketOpts = _smtp.ResolveSecureSocketOption();
                await smtp.ConnectAsync(_smtp.Host, _smtp.Port, socketOpts).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(_smtp.UserName))
                {
                    await smtp.AuthenticateAsync(_smtp.UserName, _smtp.Password).ConfigureAwait(false);
                }

                await smtp.SendAsync(mimeMessage).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", emailDto.To);
                throw new InvalidOperationException("Email could not be sent. Please verify SMTP configuration.", ex);
            }
            finally
            {
                if (smtp.IsConnected)
                    await smtp.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }
}
