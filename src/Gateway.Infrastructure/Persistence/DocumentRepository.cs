using Gateway.Application.DTOs;
using Gateway.Application.Interfaces;
using Gateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Persistence;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;

    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentAsset> GetByIdAsync(string id)
    {
        return await _context.Documents.FindAsync(id) ?? throw new KeyNotFoundException($"Document {id} not found");
    }

    public async Task<PagedResponseDto<DocumentAsset>> SearchAsync(DocumentSearchFiltersDto filters)
    {
        var query = _context.Documents.AsQueryable();

        if (filters.UploadDateStart.HasValue)
            query = query.Where(d => d.UploadDate >= filters.UploadDateStart.Value);

        if (filters.UploadDateEnd.HasValue)
            query = query.Where(d => d.UploadDate <= filters.UploadDateEnd.Value);

        if (!string.IsNullOrEmpty(filters.Filename))
            query = query.Where(d => d.Filename.Contains(filters.Filename));

        if (!string.IsNullOrEmpty(filters.ContentType))
            query = query.Where(d => d.ContentType == filters.ContentType);

        if (filters.DocumentType.HasValue)
            query = query.Where(d => d.DocumentType == filters.DocumentType.Value);

        if (filters.Status.HasValue)
            query = query.Where(d => d.Status == filters.Status.Value);

        if (!string.IsNullOrEmpty(filters.CustomerId))
            query = query.Where(d => d.CustomerId == filters.CustomerId);

        if (filters.Channel.HasValue)
            query = query.Where(d => d.Channel == filters.Channel.Value);

        query = filters.SortBy.ToLower() switch
        {
            "filename" => filters.SortDirection == "DESC" ? query.OrderByDescending(d => d.Filename) : query.OrderBy(d => d.Filename),
            "documenttype" => filters.SortDirection == "DESC" ? query.OrderByDescending(d => d.DocumentType) : query.OrderBy(d => d.DocumentType),
            "status" => filters.SortDirection == "DESC" ? query.OrderByDescending(d => d.Status) : query.OrderBy(d => d.Status),
            _ => filters.SortDirection == "DESC" ? query.OrderByDescending(d => d.UploadDate) : query.OrderBy(d => d.UploadDate),
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return new PagedResponseDto<DocumentAsset>(items, totalCount, filters.PageNumber, filters.PageSize);
    }

    public async Task AddAsync(DocumentAsset document)
    {
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(DocumentAsset document)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync();
    }
}
