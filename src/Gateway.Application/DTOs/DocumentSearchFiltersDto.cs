using Gateway.Domain.Enums;

namespace Gateway.Application.DTOs;

public class DocumentSearchFiltersDto
{
    public DateTime? UploadDateStart { get; set; }
    public DateTime? UploadDateEnd { get; set; }
    public string? Filename { get; set; }
    public string? ContentType { get; set; }
    public DocumentType? DocumentType { get; set; }
    public DocumentStatus? Status { get; set; }
    public string? CustomerId { get; set; }
    public Channel? Channel { get; set; }
    public string SortBy { get; set; } = "uploadDate";
    public string SortDirection { get; set; } = "ASC";
}
