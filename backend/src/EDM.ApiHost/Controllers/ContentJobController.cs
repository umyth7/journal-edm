using EDM.Application.Dtos.ContentJob;
using EDM.Application.Interfaces.Infrastructure;
using EDM.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EDM.ApiHost.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentJobController : ControllerBase
{
    private readonly IContentJobService _service;
    public ContentJobController(IContentJobService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await _service.GetAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        try { return Ok(await _service.GetByIdAsync(id, ct)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpGet("artist/{artistId:guid}")]
    public async Task<IActionResult> GetByArtist(Guid artistId, CancellationToken ct)
        => Ok(await _service.GetByArtistIdAsync(artistId, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ContentJobCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var created = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ContentJobUpdateDto dto, CancellationToken ct)
    {
        try { return Ok(await _service.UpdateAsync(id, dto, ct)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ContentJobStatus status, CancellationToken ct)
    {
        try { return Ok(await _service.UpdateStatusAsync(id, status, ct)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try { await _service.DeleteAsync(id, ct); return NoContent(); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
