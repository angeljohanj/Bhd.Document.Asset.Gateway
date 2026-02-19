using Gateway.Application.Interfaces;
using Gateway.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Gateway.Infrastructure.BackgroundWorkers;

public class DocumentUploadWorker : BackgroundService
{
    private readonly IUploadJobQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentUploadWorker> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public DocumentUploadWorker(
        IUploadJobQueue queue,
        IServiceProvider serviceProvider,
        ILogger<DocumentUploadWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, "Retry {RetryCount} for document upload after {TimeSpan}s due to error.", retryCount, timeSpan.TotalSeconds);
                });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Document Upload Worker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            UploadJob? job = null;
            try
            {
                job = await _queue.DequeueAsync(stoppingToken);

                _logger.LogInformation("Processing upload for document {DocumentId}", job.Document.Id);

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();

                    await SimulateUploadAsync(stoppingToken);

                    job.Document.Status = DocumentStatus.SENT;
                    job.Document.Url = $"https://internal-storage.bhd.com.do/assets/{job.Document.Id}/{job.Document.Filename}";

                    await repository.UpdateAsync(job.Document);
                });

                _logger.LogInformation("Successfully processed upload for document {DocumentId}", job.Document.Id);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Permanent failure processing document upload job after all retries.");
                
                if (job != null)
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
                        
                        job.Document.Status = DocumentStatus.FAILED;
                        await repository.UpdateAsync(job.Document);
                    }
                    catch (Exception dbEx)
                    {
                        _logger.LogError(dbEx, "Failed to update document status to FAILED for {DocumentId}", job.Document.Id);
                    }
                }
            }
        }

        _logger.LogInformation("Document Upload Worker is stopping.");
    }

    private async Task SimulateUploadAsync(CancellationToken ct)
    {
        if (Random.Shared.Next(1, 10) <= 2) 
        {
            throw new Exception("Network error.");
        }

        await Task.Delay(2000, ct); 
    }
}
