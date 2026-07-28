using EDM.Domain.Core;
using EDM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EDM.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Artist> Artists { get; set; }
    public DbSet<ContentJob> ContentJobs { get; set; }
    public DbSet<Caption> Captions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Artist>(b =>
        {
            b.HasKey(a => a.Id);
            b.HasIndex(a => a.Slug).IsUnique();
            b.HasQueryFilter(a => !a.IsDeleted);
        });

        builder.Entity<ContentJob>(b =>
        {
            b.HasKey(j => j.Id);
            b.HasOne(j => j.Artist)
             .WithMany(a => a.ContentJobs)
             .HasForeignKey(j => j.ArtistId);
            b.HasQueryFilter(j => !j.IsDeleted);
        });

        builder.Entity<Caption>(b =>
        {
            b.HasKey(c => c.Id);
            b.HasOne(c => c.ContentJob)
             .WithMany(j => j.Captions)
             .HasForeignKey(c => c.ContentJobId);
            b.HasQueryFilter(c => !c.IsDeleted);
        });

        base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity<Guid>>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                    entry.Entity.CreationTime = DateTime.UtcNow;
                    entry.Entity.UpdatedTime = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedTime = DateTime.UtcNow;
                    break;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
