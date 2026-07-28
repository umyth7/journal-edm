using EDM.Domain.Enums;

namespace EDM.Application.Dtos.ContentJob;

public class ContentJobUpdateDto
{
    public ContentJobStatus? Status { get; set; }
    public string? S3Key { get; set; }
    public DateTime? ScheduledAt { get; set; }
}
