using Gateway.Domain.Entities;

namespace Gateway.Application.Interfaces;

public interface IDocumentPublisher
{
    Task PublishAsync(DocumentAsset document, string encodedFile);
}
