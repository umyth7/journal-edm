using EDM.Domain.Entities;

namespace EDM.Application.Interfaces.Persistence;

public interface ICaptionRepository
{
    Task<Caption> CreateAsync(Caption caption, CancellationToken cancellationToken);
    Task<List<Caption>> GetByContentJobIdAsync(Guid contentJobId, CancellationToken cancellationToken);
}
