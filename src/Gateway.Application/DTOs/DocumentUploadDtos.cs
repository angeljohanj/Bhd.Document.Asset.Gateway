using Gateway.Domain.Enums;

namespace Gateway.Application.DTOs;

public class DocumentUploadRequestDto
{
    public string Filename { get; set; } = string.Empty;
    public string EncodedFile { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public Channel Channel { get; set; }
    public string? CustomerId { get; set; }
    public string? CorrelationId { get; set; }
}

public class DocumentUploadResponseDto
{
    public string Id { get; set; } = string.Empty;
}
