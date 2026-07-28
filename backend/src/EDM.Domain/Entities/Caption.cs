using EDM.Domain.Core;
using EDM.Domain.Enums;

namespace EDM.Domain.Entities;

public class Caption : BaseEntity<Guid>
{
    public Guid ContentJobId { get; set; }
    public Platform Platform { get; set; }
    public string CaptionText { get; set; } = string.Empty;
    public string Hashtags { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public ContentJob ContentJob { get; set; } = null!;
}
