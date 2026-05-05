using System.Threading.Channels;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Services.Auth;

namespace WebApiEcomm.InfraStructure.Services.Auth
{
    public class EmailQueue : IEmailQueue
    {
        private readonly Channel<EmailDto> _queue;

        public EmailQueue()
        {
            _queue = Channel.CreateBounded<EmailDto>(new BoundedChannelOptions(200)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
        }

        public ValueTask QueueAsync(EmailDto email, CancellationToken cancellationToken = default)
            => _queue.Writer.WriteAsync(email, cancellationToken);

        public IAsyncEnumerable<EmailDto> DequeueAllAsync(CancellationToken cancellationToken)
            => _queue.Reader.ReadAllAsync(cancellationToken);
    }
}
