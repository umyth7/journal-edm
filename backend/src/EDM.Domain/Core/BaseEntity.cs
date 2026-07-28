namespace EDM.Domain.Core;

public abstract class BaseEntity<TId>
{
    public TId Id { get; set; } = default!;
    public DateTime CreationTime { get; set; }
    public DateTime UpdatedTime { get; set; }
    public DateTime? DeletionTime { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsActive { get; set; } = true;
}
