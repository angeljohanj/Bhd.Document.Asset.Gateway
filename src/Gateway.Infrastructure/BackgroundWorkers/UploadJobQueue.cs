using System.Threading.Channels;
using Gateway.Domain.Entities;

namespace Gateway.Infrastructure.BackgroundWorkers;

public record UploadJob(DocumentAsset Document, string EncodedFile);

public interface IUploadJobQueue
{
    ValueTask EnqueueAsync(UploadJob job);
    ValueTask<UploadJob> DequeueAsync(CancellationToken cancellationToken);
}

public class UploadJobQueue : IUploadJobQueue
{
    private readonly Channel<UploadJob> _queue;

    public UploadJobQueue(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<UploadJob>(options);
    }

    public async ValueTask EnqueueAsync(UploadJob job)
    {
        await _queue.Writer.WriteAsync(job);
    }

    public async ValueTask<UploadJob> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
