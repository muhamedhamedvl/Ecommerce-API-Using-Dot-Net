using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using WebApiEcomm.Core.Services;

namespace WebApiEcomm.InfraStructure.Services.Auth
{
    public class EmailBackgroundWorker : BackgroundService
    {
        private readonly EmailQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailBackgroundWorker> _logger;

        public EmailBackgroundWorker(
            EmailQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<EmailBackgroundWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var email in _queue.DequeueAllAsync(stoppingToken))
            {
                var sent = false;
                for (var retry = 0; retry < 3 && !sent; retry++)
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        await emailService.SendEmailAsync(email, stoppingToken);
                        sent = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Email send failed for {To} on retry {Retry}", email.To, retry + 1);
                        if (!stoppingToken.IsCancellationRequested)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retry + 1)), stoppingToken);
                        }
                    }
                }

                if (!sent)
                {
                    _logger.LogError("Email permanently failed for {To} after retries.", email.To);
                }
            }
        }
    }
}
