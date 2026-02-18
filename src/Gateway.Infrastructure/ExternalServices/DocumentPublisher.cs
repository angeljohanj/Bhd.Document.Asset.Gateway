using Gateway.Application.Interfaces;
using Gateway.Domain.Entities;
using Gateway.Infrastructure.BackgroundWorkers;

namespace Gateway.Infrastructure.ExternalServices;

public class DocumentPublisher : IDocumentPublisher
{
    private readonly IUploadJobQueue _queue;

    public DocumentPublisher(IUploadJobQueue queue)
    {
        _queue = queue;
    }

    public async Task PublishAsync(DocumentAsset document, string encodedFile)
    {
        await _queue.EnqueueAsync(new UploadJob(document, encodedFile));
    }
}
