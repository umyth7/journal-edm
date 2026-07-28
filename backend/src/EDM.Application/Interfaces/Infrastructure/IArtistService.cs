using EDM.Application.Dtos.Artist;

namespace EDM.Application.Interfaces.Infrastructure;

public interface IArtistService
{
    Task<List<ArtistResponseDto>> GetAsync(CancellationToken cancellationToken);
    Task<ArtistResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ArtistResponseDto> CreateAsync(ArtistCreateDto dto, CancellationToken cancellationToken);
    Task<ArtistResponseDto> UpdateAsync(Guid id, ArtistUpdateDto dto, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<ArtistResponseDto> OverrideScoreAsync(Guid id, int score, bool dt, CancellationToken cancellationToken);
}
