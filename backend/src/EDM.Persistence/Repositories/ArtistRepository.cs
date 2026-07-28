using EDM.Application.Interfaces.Persistence;
using EDM.Domain.Entities;
using EDM.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EDM.Persistence.Repositories;

public class ArtistRepository : IArtistRepository
{
    private readonly ApplicationDbContext _db;
    public ArtistRepository(ApplicationDbContext db) => _db = db;

    public IQueryable<Artist> GetQuery() => _db.Artists.AsQueryable().AsNoTracking();

    public async Task<Artist?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _db.Artists.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Artist?> GetBySlugAsync(string slug, CancellationToken ct)
        => await _db.Artists.AsNoTracking().FirstOrDefaultAsync(a => a.Slug == slug, ct);

    public async Task<List<Artist>> GetAsync(CancellationToken ct)
        => await _db.Artists.AsNoTracking().ToListAsync(ct);

    public async Task<Artist> CreateAsync(Artist artist, CancellationToken ct)
    {
        await _db.Artists.AddAsync(artist, ct);
        await _db.SaveChangesAsync(ct);
        return artist;
    }

    public async Task<Artist> UpdateAsync(Artist artist, CancellationToken ct)
    {
        _db.Artists.Update(artist);
        await _db.SaveChangesAsync(ct);
        return artist;
    }

    public async Task DeleteAsync(Artist artist, CancellationToken ct)
    {
        artist.IsDeleted = true;
        artist.DeletionTime = DateTime.UtcNow;
        _db.Artists.Update(artist);
        await _db.SaveChangesAsync(ct);
    }
}
