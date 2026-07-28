using EDM.Application.Dtos.Artist;
using EDM.Application.Interfaces.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace EDM.ApiHost.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistController : ControllerBase
{
    private readonly IArtistService _service;
    private readonly IArtistScoreService _scoreService;

    public ArtistController(IArtistService service, IArtistScoreService scoreService)
    {
        _service = service;
        _scoreService = scoreService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await _service.GetAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        try { return Ok(await _service.GetByIdAsync(id, ct)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ArtistCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ArtistUpdateDto dto, CancellationToken ct)
    {
        try { return Ok(await _service.UpdateAsync(id, dto, ct)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try { await _service.DeleteAsync(id, ct); return NoContent(); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>
    /// Manuel skor override — ManualOverride=true yapar, otomatik hesaplama devre dışı kalır
    /// </summary>
    [HttpPatch("{id:guid}/score")]
    public async Task<IActionResult> OverrideScore(Guid id, [FromBody] ScoreOverrideDto dto, CancellationToken ct)
    {
        if (dto.Score < 1 || dto.Score > 100)
            return BadRequest(new { message = "Score 1-100 arasında olmalıdır" });

        try
        {
            var updated = await _service.OverrideScoreAsync(id, dto.Score, dt: dto.ClearManualOverride, ct);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>
    /// Stored metriklerden skoru yeniden hesapla (ManualOverride aktifse çalışmaz)
    /// </summary>
    [HttpPost("{id:guid}/score/recalculate")]
    public async Task<IActionResult> RecalculateScore(Guid id, CancellationToken ct)
    {
        try
        {
            await _scoreService.RecalculateAsync(id, ct);
            return Ok(await _service.GetByIdAsync(id, ct));
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>
    /// Tüm artistlerin skorunu yeniden hesapla (ManualOverride aktif olanlar hariç)
    /// </summary>
    [HttpPost("score/recalculate-all")]
    public async Task<IActionResult> RecalculateAllScores(CancellationToken ct)
    {
        await _scoreService.RecalculateAllAsync(ct);
        return Ok(new { message = "Tüm artist skorları güncellendi" });
    }
}

public record ScoreOverrideDto(int Score, bool ClearManualOverride = false);
