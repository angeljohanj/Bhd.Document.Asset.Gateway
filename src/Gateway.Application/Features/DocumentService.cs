using Gateway.Application.DTOs;
using Gateway.Application.Interfaces;
using Gateway.Domain.Entities;
using Gateway.Domain.Enums;

namespace Gateway.Application.Features;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly IDocumentPublisher _publisher;

    public DocumentService(IDocumentRepository repository, IDocumentPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<DocumentUploadResponseDto> UploadDocumentAsync(DocumentUploadRequestDto request)
    {

        var fileSize = CalculateBase64Size(request.EncodedFile);

        var documentAsset = new DocumentAsset
        {
            Filename = request.Filename,
            ContentType = request.ContentType,
            DocumentType = request.DocumentType,
            Channel = request.Channel,
            CustomerId = request.CustomerId,
            CorrelationId = request.CorrelationId,
            Size = fileSize,
            Status = DocumentStatus.RECEIVED,
            UploadDate = DateTime.UtcNow
        };

        await _repository.AddAsync(documentAsset);

        _ = _publisher.PublishAsync(documentAsset, request.EncodedFile);

        return new DocumentUploadResponseDto { Id = documentAsset.Id };
    }

    public async Task<PagedResponseDto<DocumentAsset>> SearchDocumentsAsync(DocumentSearchFiltersDto filters)
    {
        return await _repository.SearchAsync(filters);
    }

    private long CalculateBase64Size(string base64String)
    {
        if (string.IsNullOrEmpty(base64String)) return 0;

        var base64Data = base64String.Contains(",") ? base64String.Split(',')[1] : base64String;
        return (long)(base64Data.Length * 0.75);
    }
}
