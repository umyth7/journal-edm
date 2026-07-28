using EDM.Application.Dtos.ContentJob;
using EDM.Application.Interfaces.Infrastructure;
using EDM.Application.Interfaces.Persistence;
using EDM.Domain.Entities;
using EDM.Domain.Enums;

namespace EDM.Infrastructure.Services;

public class ContentJobService : IContentJobService
{
    private readonly IContentJobRepository _repo;
    private readonly IArtistRepository _artistRepo;
    private readonly IFilenameParser _parser;

    public ContentJobService(IContentJobRepository repo, IArtistRepository artistRepo, IFilenameParser parser)
    {
        _repo = repo;
        _artistRepo = artistRepo;
        _parser = parser;
    }

    public async Task<List<ContentJobResponseDto>> GetAsync(CancellationToken ct)
    {
        var jobs = await _repo.GetAllAsync(ct);
        return jobs.Select(MapToDto).ToList();
    }

    public async Task<ContentJobResponseDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var job = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"ContentJob bulunamadi: {id}");
        return MapToDto(job);
    }

    public async Task<List<ContentJobResponseDto>> GetByArtistIdAsync(Guid artistId, CancellationToken ct)
        => (await _repo.GetByArtistIdAsync(artistId, ct)).Select(MapToDto).ToList();

    public async Task<ContentJobResponseDto> CreateAsync(ContentJobCreateDto dto, CancellationToken ct)
    {
        var parseResult = _parser.Parse(dto.RawFileName);
        if (!parseResult.Success)
            throw new ArgumentException($"Dosya adi parse hatasi: {parseResult.ErrorMessage}");

        var job = new ContentJob
        {
            ArtistId = dto.ArtistId,
            RawFileName = dto.RawFileName,
            Year = parseResult.Year,
            EventName = parseResult.EventName,
            Status = ContentJobStatus.Pending
        };
        var created = await _repo.CreateAsync(job, ct);
        // Artist navigation yukle
        var withArtist = await _repo.GetByIdAsync(created.Id, ct);
        return MapToDto(withArtist!);
    }

    public async Task<ContentJobResponseDto> UpdateAsync(Guid id, ContentJobUpdateDto dto, CancellationToken ct)
    {
        var job = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"ContentJob bulunamadi: {id}");
        if (dto.Status.HasValue) job.Status = dto.Status.Value;
        if (dto.S3Key != null) job.S3Key = dto.S3Key;
        if (dto.ScheduledAt.HasValue) job.ScheduledAt = dto.ScheduledAt.Value;
        var updated = await _repo.UpdateAsync(job, ct);
        return MapToDto(updated);
    }

    public async Task<ContentJobResponseDto> UpdateStatusAsync(Guid id, ContentJobStatus status, CancellationToken ct)
    {
        var job = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"ContentJob bulunamadi: {id}");
        job.Status = status;
        if (status == ContentJobStatus.Published) job.PublishedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(job, ct);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var job = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"ContentJob bulunamadi: {id}");
        await _repo.DeleteAsync(job, ct);
    }

    private static ContentJobResponseDto MapToDto(ContentJob j) => new()
    {
        Id = j.Id,
        ArtistId = j.ArtistId,
        ArtistName = j.Artist?.Name ?? string.Empty,
        Year = j.Year,
        EventName = j.EventName,
        RawFileName = j.RawFileName,
        S3Key = j.S3Key,
        Status = j.Status,
        ScheduledAt = j.ScheduledAt,
        PublishedAt = j.PublishedAt,
        InstagramPostId = j.InstagramPostId,
        TikTokPostId = j.TikTokPostId,
        YoutubeVideoId = j.YoutubeVideoId,
        RetryCount = j.RetryCount,
        CreatedAt = j.CreationTime
    };
}
