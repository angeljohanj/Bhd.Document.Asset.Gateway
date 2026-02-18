using Gateway.Application.DTOs;
using Gateway.Application.Interfaces;
using Gateway.Domain.Entities;
using Gateway.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Api.Controllers;

[ApiController]
[Route("api/bhd/mgmt/1/documents")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }
    /// <summary>
    /// Accepts a document upload request and triggers an asynchronous upload process.
    /// Receives the document payload, persists metadata, and delegates the upload to an internal publisher asynchronously.
    /// This service is not the final repository of the document.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("actions/upload")]
    [ProducesResponseType(typeof(DocumentUploadResponseDto), 202)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UploadDocument([FromBody] DocumentUploadRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _documentService.UploadDocumentAsync(request);
        return Accepted(response);
    }
    /// <summary>
    /// Search and filter uploaded documents based on persisted metadata.
    /// </summary>
    /// <param name="uploadDateStart"></param>
    /// <param name="uploadDateEnd"></param>
    /// <param name="filename"></param>
    /// <param name="contentType"></param>
    /// <param name="documentType"></param>
    /// <param name="status"></param>
    /// <param name="customerId"></param>
    /// <param name="channel"></param>
    /// <param name="sortBy"></param>
    /// <param name="sortDirection"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocumentAsset>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SearchDocuments(
        [FromQuery] DateTime? uploadDateStart,
        [FromQuery] DateTime? uploadDateEnd,
        [FromQuery] string? filename,
        [FromQuery] string? contentType,
        [FromQuery] DocumentType? documentType,
        [FromQuery] DocumentStatus? status,
        [FromQuery] string? customerId,
        [FromQuery] Channel? channel,
        [FromQuery] string sortBy = "uploadDate",
        [FromQuery] string sortDirection = "ASC")
    {
        var filters = new DocumentSearchFiltersDto
        {
            UploadDateStart = uploadDateStart,
            UploadDateEnd = uploadDateEnd,
            Filename = filename,
            ContentType = contentType,
            DocumentType = documentType,
            Status = status,
            CustomerId = customerId,
            Channel = channel,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var results = await _documentService.SearchDocumentsAsync(filters);
        return Ok(results);
    }
}
