namespace PurchaseApp.Services;

public sealed class PurchaseRequestApiWorker : BackgroundService
{
    private readonly PurchaseRequestApiQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PurchaseRequestApiWorker> _logger;

    public PurchaseRequestApiWorker(
        PurchaseRequestApiQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<PurchaseRequestApiWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workers = Enumerable.Range(0, 4).Select(_ => ProcessQueueAsync(stoppingToken));
        await Task.WhenAll(workers);
    }

    private async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        await foreach (var requestId in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<PurchaseRequestApiService>();
                await processor.ProcessAsync(requestId);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to process purchase request {RequestId}", requestId);
            }
        }
    }
}
