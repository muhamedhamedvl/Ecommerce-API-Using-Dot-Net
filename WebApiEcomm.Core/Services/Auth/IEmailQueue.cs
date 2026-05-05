using WebApiEcomm.Core.Entites.Dtos;

namespace WebApiEcomm.Core.Services.Auth
{
    public interface IEmailQueue
    {
        ValueTask QueueAsync(EmailDto email, CancellationToken cancellationToken = default);
    }
}
