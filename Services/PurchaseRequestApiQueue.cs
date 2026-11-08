using System.Threading.Channels;

namespace PurchaseApp.Services;

public sealed class PurchaseRequestApiQueue
{
    private readonly Channel<int> _queue = Channel.CreateUnbounded<int>();

    public ValueTask EnqueueAsync(int requestId) => _queue.Writer.WriteAsync(requestId);

    public IAsyncEnumerable<int> ReadAllAsync(CancellationToken cancellationToken) =>
        _queue.Reader.ReadAllAsync(cancellationToken);
}
