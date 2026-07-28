using EDM.Domain.Entities;
using EDM.Domain.Enums;

namespace EDM.Application.Interfaces.Infrastructure;

public interface ICaptionService
{
    Task<Caption> GenerateAsync(string artistName, string eventName, int year, Platform platform, CancellationToken cancellationToken);
    Task<List<Caption>> GenerateAllPlatformsAsync(string artistName, string eventName, int year, Guid contentJobId, CancellationToken cancellationToken);
}
