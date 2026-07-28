using EDM.Application.Interfaces.Persistence;
using EDM.Domain.Entities;
using EDM.Domain.Enums;
using EDM.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EDM.Persistence.Repositories;

public class ContentJobRepository : IContentJobRepository
{
    private readonly ApplicationDbContext _db;
    public ContentJobRepository(ApplicationDbContext db) => _db = db;

    public IQueryable<ContentJob> GetQuery() => _db.ContentJobs.AsQueryable().AsNoTracking();

    public async Task<ContentJob?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _db.ContentJobs.Include(j => j.Artist).Include(j => j.Captions)
            .AsNoTracking().FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<List<ContentJob>> GetByArtistIdAsync(Guid artistId, CancellationToken ct)
        => await _db.ContentJobs.Include(j => j.Artist)
            .AsNoTracking().Where(j => j.ArtistId == artistId).ToListAsync(ct);

    public async Task<List<ContentJob>> GetByStatusAsync(ContentJobStatus status, CancellationToken ct)
        => await _db.ContentJobs.Include(j => j.Artist)
            .AsNoTracking().Where(j => j.Status == status).ToListAsync(ct);

    public async Task<List<ContentJob>> GetAllAsync(CancellationToken ct)
        => await _db.ContentJobs.Include(j => j.Artist).AsNoTracking().ToListAsync(ct);

    public async Task<ContentJob> CreateAsync(ContentJob job, CancellationToken ct)
    {
        await _db.ContentJobs.AddAsync(job, ct);
        await _db.SaveChangesAsync(ct);
        return job;
    }

    public async Task<ContentJob> UpdateAsync(ContentJob job, CancellationToken ct)
    {
        _db.ContentJobs.Update(job);
        await _db.SaveChangesAsync(ct);
        return job;
    }

    public async Task DeleteAsync(ContentJob job, CancellationToken ct)
    {
        job.IsDeleted = true;
        job.DeletionTime = DateTime.UtcNow;
        _db.ContentJobs.Update(job);
        await _db.SaveChangesAsync(ct);
    }
}
