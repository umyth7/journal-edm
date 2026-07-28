using EDM.Domain.Entities;

namespace EDM.Application.Interfaces.Persistence;

public interface IArtistRepository
{
    IQueryable<Artist> GetQuery();
    Task<Artist?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Artist?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<List<Artist>> GetAsync(CancellationToken cancellationToken);
    Task<Artist> CreateAsync(Artist artist, CancellationToken cancellationToken);
    Task<Artist> UpdateAsync(Artist artist, CancellationToken cancellationToken);
    Task DeleteAsync(Artist artist, CancellationToken cancellationToken);
}
