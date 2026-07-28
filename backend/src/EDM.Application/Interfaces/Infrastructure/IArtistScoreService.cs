namespace EDM.Application.Interfaces.Infrastructure;

public interface IArtistScoreService
{
    int CalculateScore(long youtubeViews90d, long instagramReach90d, long tiktokPlays90d, long spotifyListeners);
    Task RecalculateAsync(Guid artistId, CancellationToken cancellationToken);
    Task RecalculateAllAsync(CancellationToken cancellationToken);
}
