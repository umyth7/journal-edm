using System.ComponentModel.DataAnnotations;

namespace EDM.Application.Dtos.ContentJob;

public class ContentJobCreateDto
{
    [Required]
    public Guid ArtistId { get; set; }
    [Required][MaxLength(500)]
    public string RawFileName { get; set; } = string.Empty;
}
