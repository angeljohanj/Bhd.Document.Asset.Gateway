using Gateway.Application.Interfaces;
using Gateway.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gateway.Infrastructure.BackgroundWorkers;

public class DocumentUploadWorker : BackgroundService
{
    private readonly IUploadJobQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentUploadWorker> _logger;

    public DocumentUploadWorker(
        IUploadJobQueue queue,
        IServiceProvider serviceProvider,
        ILogger<DocumentUploadWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Document Upload Worker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _queue.DequeueAsync(stoppingToken);

                _logger.LogInformation("Processing upload for document {DocumentId}", job.Document.Id);

                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();

                await Task.Delay(2000, stoppingToken);

                job.Document.Status = DocumentStatus.SENT;
                job.Document.Url = $"https://internal-storage.bhd.com.do/assets/{job.Document.Id}/{job.Document.Filename}";

                await repository.UpdateAsync(job.Document);

                _logger.LogInformation("Successfully processed upload for document {DocumentId}", job.Document.Id);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing document upload job.");
            }
        }

        _logger.LogInformation("Document Upload Worker is stopping.");
    }
}
