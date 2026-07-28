using EDM.Application.Dtos.ContentJob;
using EDM.Domain.Enums;

namespace EDM.Application.Interfaces.Infrastructure;

public interface IContentJobService
{
    Task<List<ContentJobResponseDto>> GetAsync(CancellationToken cancellationToken);
    Task<ContentJobResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ContentJobResponseDto>> GetByArtistIdAsync(Guid artistId, CancellationToken cancellationToken);
    Task<ContentJobResponseDto> CreateAsync(ContentJobCreateDto dto, CancellationToken cancellationToken);
    Task<ContentJobResponseDto> UpdateAsync(Guid id, ContentJobUpdateDto dto, CancellationToken cancellationToken);
    Task<ContentJobResponseDto> UpdateStatusAsync(Guid id, ContentJobStatus status, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
