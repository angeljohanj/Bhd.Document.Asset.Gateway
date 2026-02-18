using Gateway.Domain.Entities;
using Gateway.Application.DTOs;

namespace Gateway.Application.Interfaces;

public interface IDocumentRepository
{
    Task<DocumentAsset> GetByIdAsync(string id);
    Task<IEnumerable<DocumentAsset>> SearchAsync(DocumentSearchFiltersDto filters);
    Task AddAsync(DocumentAsset document);
    Task UpdateAsync(DocumentAsset document);
}
