using EDM.Domain.Entities;
using EDM.Domain.Enums;

namespace EDM.Application.Interfaces.Persistence;

public interface IContentJobRepository
{
    IQueryable<ContentJob> GetQuery();
    Task<ContentJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ContentJob>> GetByArtistIdAsync(Guid artistId, CancellationToken cancellationToken);
    Task<List<ContentJob>> GetByStatusAsync(ContentJobStatus status, CancellationToken cancellationToken);
    Task<List<ContentJob>> GetAllAsync(CancellationToken cancellationToken);
    Task<ContentJob> CreateAsync(ContentJob job, CancellationToken cancellationToken);
    Task<ContentJob> UpdateAsync(ContentJob job, CancellationToken cancellationToken);
    Task DeleteAsync(ContentJob job, CancellationToken cancellationToken);
}
