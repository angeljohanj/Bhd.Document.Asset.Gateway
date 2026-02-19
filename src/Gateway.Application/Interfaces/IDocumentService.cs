using Gateway.Application.DTOs;
using Gateway.Domain.Entities;

namespace Gateway.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentUploadResponseDto> UploadDocumentAsync(DocumentUploadRequestDto request);
    Task<PagedResponseDto<DocumentAsset>> SearchDocumentsAsync(DocumentSearchFiltersDto filters);
}
