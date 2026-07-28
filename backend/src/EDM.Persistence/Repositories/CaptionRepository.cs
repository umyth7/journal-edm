using EDM.Application.Interfaces.Persistence;
using EDM.Domain.Entities;
using EDM.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EDM.Persistence.Repositories;

public class CaptionRepository : ICaptionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CaptionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Caption> CreateAsync(Caption caption, CancellationToken cancellationToken)
    {
        await _dbContext.Captions.AddAsync(caption, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return caption;
    }

    public async Task<List<Caption>> GetByContentJobIdAsync(Guid contentJobId, CancellationToken cancellationToken)
        => await _dbContext.Captions
            .Where(c => c.ContentJobId == contentJobId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
