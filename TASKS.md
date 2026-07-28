# Sprint 1 Görev Listesi — EDM Journal Automation Platform
**Tarih:** 2026-07-27  
**Sprint Hedefi:** Temel altyapı, domain modeli, veritabanı ve CRUD API'nin çalışır hale getirilmesi

---

## Genel Mimari

```
EDM.ApiHost          → ASP.NET Core Web API (giriş noktası)
EDM.Application      → DTOs, Interfaces, Service tanımları
EDM.Domain           → Entity'ler, Enum'lar, Base sınıflar
EDM.Infrastructure   → Servis implementasyonları (FilenameParser, SchedulingService)
EDM.Persistence      → DbContext, Repository'ler, EF Core config, Migration
```

Template kaynak: `D:\GIT\enliq\enliq-campaign-management-main\enliq-campaign-management-main\`  
Namespace dönüşümü: `EIQ.Campaign.*` → `EDM.*`

---

## Bekleyen

---

### TASK-001 — Şablon Kopyalama ve Solution Oluşturma

**Başlık:** Template projeden EDM solution'ını türet, namespace'leri yeniden adlandır

**Açıklama:**  
Template kaynak projeyi (`D:\GIT\enliq\...`) `C:\Users\Kullanıcı\Source\journal-edm\src\` altına kopyala. Tüm dosyalarda `EIQ.Campaign` → `EDM` namespace dönüşümünü uygula. Gereksiz entity ve enum dosyalarını sil; EDM projesine ait olanlarla değiştir.

**Yapılacaklar:**

1. `C:\Users\Kullanıcı\Source\journal-edm\src\` klasörü oluştur
2. Aşağıdaki proje klasörlerini kopyala ve yeniden adlandır:

| Kaynak | Hedef |
|--------|-------|
| `EIQ.Campaign.ApiHost` | `EDM.ApiHost` |
| `EIQ.Campaign.Application` | `EDM.Application` |
| `EIQ.Campaign.Domain` | `EDM.Domain` |
| `EIQ.Campaign.Infrastructure` | `EDM.Infrastructure` |
| `EIQ.Campaign.Persistence` | `EDM.Persistence` |

> NOT: `EIQ.Campaign.BackgroundTasks`, `EIQ.Campaign.IntegrationApi`, `EIQ.Campaign.IntegrationHandlers`, `EIQ.Campaign.Provider`, `EIQ.Campaign.Core` projeleri Sprint 1'de **dahil edilmez**.

3. Her `*.csproj` dosyasında proje adını güncelle (örn. `EIQ.Campaign.ApiHost.csproj` → `EDM.ApiHost.csproj`)
4. Tüm `.cs` dosyalarında toplu namespace değişimi yap:
   - `EIQ.Campaign.Domain` → `EDM.Domain`
   - `EIQ.Campaign.Application` → `EDM.Application`
   - `EIQ.Campaign.Persistence` → `EDM.Persistence`
   - `EIQ.Campaign.Infrastructure` → `EDM.Infrastructure`
   - `EIQ.Campaign.ApiHost` → `EDM.ApiHost`
   - `EIQ.Campaign` → `EDM` (genel kapsamda)
5. `EDM.sln` solution dosyasını oluştur ve tüm projeleri ekle
6. `EIQ.Claim`, `Lds.Cache`, `Lds.Core` gibi şirkete özel NuGet paketlerini **sil veya stub'la** — bu bağımlılıklar Sprint 1'de yoktur; authentication middleware `[AllowAnonymous]` ile bypass edilebilir

**Bağımlılıklar:** Yok (başlangıç görevi)

**Tahmini Süre:** 2 saat

**Kabul Kriterleri:**
- `dotnet build` komutu hatasız tamamlanır
- Solution'da 5 proje bulunur
- `EIQ` veya `Campaign` string'i hiçbir `.cs` ya da `.csproj` dosyasında **kalmaz**
- `dotnet run --project src/EDM.ApiHost` ile uygulama ayağa kalkar (Swagger UI erişilebilir)

---

### TASK-002 — Domain Entity'leri ve Enum'lar

**Başlık:** Artist, ContentJob, Caption entity'leri ve ilgili enum'ları oluştur

**Açıklama:**  
`EDM.Domain` projesi altında yeni entity sınıfları ve enum'lar oluştur. Template'deki `Campaign.cs` entity yapısını ve `BaseEntity<T>` kalıtımını baz al.

**Dosyalar:**

```
src/EDM.Domain/
├── Core/
│   ├── BaseEntity.cs       (template'den kopyalandı, namespace değiştirildi)
│   ├── IEntity.cs
│   └── ISoftDelete.cs
├── Entities/
│   ├── Artist.cs           (YENİ)
│   ├── ContentJob.cs       (YENİ)
│   └── Caption.cs          (YENİ)
└── Enums/
    ├── ContentJobStatus.cs  (YENİ)
    └── Platform.cs          (YENİ)
```

**Kod — `src/EDM.Domain/Enums/ContentJobStatus.cs`:**
```csharp
namespace EDM.Domain.Enums;

public enum ContentJobStatus
{
    Pending = 0,
    Processing = 1,
    CaptionGenerated = 2,
    Scheduled = 3,
    Published = 4,
    Failed = 5
}
```

**Kod — `src/EDM.Domain/Enums/Platform.cs`:**
```csharp
namespace EDM.Domain.Enums;

public enum Platform
{
    Instagram = 0,
    TikTok = 1,
    YouTube = 2
}
```

**Kod — `src/EDM.Domain/Entities/Artist.cs`:**
```csharp
using EDM.Domain.Core;

namespace EDM.Domain.Entities;

public class Artist : BaseEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Score { get; set; }                         // 1-100
    public bool ManualOverride { get; set; } = false;
    public DateTime LastScoreUpdate { get; set; }
    public long YoutubeViews90d { get; set; }
    public long InstagramReach90d { get; set; }
    public long TikTokPlays90d { get; set; }
    public long SpotifyListeners { get; set; }

    // Navigation
    public ICollection<ContentJob> ContentJobs { get; set; } = new List<ContentJob>();
}
```

**Kod — `src/EDM.Domain/Entities/ContentJob.cs`:**
```csharp
using EDM.Domain.Core;
using EDM.Domain.Enums;

namespace EDM.Domain.Entities;

public class ContentJob : BaseEntity<Guid>
{
    public Guid ArtistId { get; set; }
    public int Year { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string RawFileName { get; set; } = string.Empty;
    public string S3Key { get; set; } = string.Empty;
    public ContentJobStatus Status { get; set; } = ContentJobStatus.Pending;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? InstagramPostId { get; set; }
    public string? TikTokPostId { get; set; }
    public string? YoutubeVideoId { get; set; }
    public int RetryCount { get; set; } = 0;

    // Navigation
    public Artist Artist { get; set; } = null!;
    public ICollection<Caption> Captions { get; set; } = new List<Caption>();
}
```

**Kod — `src/EDM.Domain/Entities/Caption.cs`:**
```csharp
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

    // Navigation
    public ContentJob ContentJob { get; set; } = null!;
}
```

**NOT:** `BaseEntity<T>` içindeki `CreationTime`/`UpdatedTime` alanları, proje veri modelindeki `CreatedAt`/`UpdatedAt` adlarına karşılık gelir. Template'deki isimler korunabilir ya da `BaseEntity` override edilerek değiştirilebilir. Sprint 1'de template isimleri (`CreationTime`, `UpdatedTime`) kullanmak yeterlidir.

**Bağımlılıklar:** TASK-001

**Tahmini Süre:** 1.5 saat

**Kabul Kriterleri:**
- `EDM.Domain` projesi `dotnet build` ile derlenir
- 3 entity sınıfı ve 2 enum dosyası mevcuttur
- Her entity `BaseEntity<Guid>` kalıtımı yapar
- Navigation property'ler doğru FK ilişkilerini yansıtır

---

### TASK-003 — DbContext ve EF Core Konfigürasyonu

**Başlık:** ApplicationDbContext oluştur, entity konfigürasyonlarını ve soft-delete filtrelerini uygula

**Açıklama:**  
Template'deki `ApplicationDbContext.cs` ve `DbContextExtensions.cs` yapısını EDM entity'lerine göre yeniden yaz. PostgreSQL + snake_case naming convention kullanılacak. Enum'lar postgres native enum olarak değil, `int` (ya da `string`) olarak saklanacak — bu Sprint 1'i basitleştirir.

**Dosyalar:**

```
src/EDM.Persistence/
├── Context/
│   ├── ApplicationDbContext.cs     (YENİ)
│   └── MigrateSettings/
│       ├── MigrationManager.cs     (template'den kopyala, namespace güncelle)
│       └── MigrationProjectAssembly.cs
├── Extensions/
│   ├── BuilderExtensions.cs
│   └── DbContextExtensions.cs     (template'den kopyala, namespace güncelle)
└── Configuration/
    └── Registration.cs             (YENİ — DI kayıt)
```

**Kod — `src/EDM.Persistence/Context/ApplicationDbContext.cs`:**
```csharp
using EDM.Domain.Core;
using EDM.Domain.Entities;
using EDM.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EDM.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Artist> Artists { get; set; }
    public DbSet<ContentJob> ContentJobs { get; set; }
    public DbSet<Caption> Captions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Artist>(b =>
        {
            b.ConfigureProperties();
            b.HasIndex(a => a.Slug).IsUnique();
        });

        builder.Entity<ContentJob>(b =>
        {
            b.ConfigureProperties();
            b.HasOne(j => j.Artist)
             .WithMany(a => a.ContentJobs)
             .HasForeignKey(j => j.ArtistId);
        });

        builder.Entity<Caption>(b =>
        {
            b.ConfigureProperties();
            b.HasOne(c => c.ContentJob)
             .WithMany(j => j.Captions)
             .HasForeignKey(c => c.ContentJobId);
        });

        builder.ConfigureSoftDelete();
        base.OnModelCreating(builder);
    }

    // SaveChanges override'ları — template'deki OnBeforeSaving() metodunu kopyala
    // (CreationTime, UpdatedTime, DeletionTime otomatik set edilir)
}
```

**Kod — `src/EDM.Persistence/Configuration/Registration.cs`:**
```csharp
using EDM.Application.Interfaces.Persistence;
using EDM.Persistence.Context;
using EDM.Persistence.Context.MigrateSettings;
using EDM.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EDM.Persistence.Configuration;

public static class Registration
{
    public static IServiceCollection UsePersistence(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("Default"),
                m =>
                {
                    m.MigrationsAssembly(typeof(MigrationProjectAssembly).Assembly.FullName);
                    m.MigrationsHistoryTable("__efcore_migrations");
                    m.EnableRetryOnFailure();
                }
            ).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IArtistRepository, ArtistRepository>();
        services.AddScoped<IContentJobRepository, ContentJobRepository>();

        return services;
    }
}
```

**`appsettings.json` bağlantı dizesi (EDM.ApiHost):**
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=edm_journal;Username=postgres;Password=your_password"
  }
}
```

**Bağımlılıklar:** TASK-002

**Tahmini Süre:** 1.5 saat

**Kabul Kriterleri:**
- `ApplicationDbContext` 3 `DbSet` içerir
- `SaveChangesAsync` override'ı `CreationTime` ve `UpdatedTime` alanlarını otomatik doldurur
- Soft-delete global query filter `IsDeleted == false` koşulunu ekler
- `Registration.cs` `IServiceCollection` extension olarak DI'a kayıt eder
- `dotnet build` hatasız geçer

---

### TASK-004 — Repository Interface'leri ve Implementasyonları

**Başlık:** IArtistRepository, IContentJobRepository interface'lerini ve implementasyonlarını yaz

**Açıklama:**  
Template'deki `ICampaignRepository` ve `CampaignRepository` yapısını EDM entity'lerine göre uygula. Repository'ler `ApplicationDbContext` üzerinden EF Core sorguları çalıştırır.

**Dosyalar:**

```
src/EDM.Application/Interfaces/Persistence/
├── IArtistRepository.cs        (YENİ)
└── IContentJobRepository.cs    (YENİ)

src/EDM.Persistence/Repositories/
├── ArtistRepository.cs         (YENİ)
└── ContentJobRepository.cs     (YENİ)
```

**Kod — `src/EDM.Application/Interfaces/Persistence/IArtistRepository.cs`:**
```csharp
using EDM.Domain.Entities;

namespace EDM.Application.Interfaces.Persistence;

public interface IArtistRepository
{
    IQueryable<Artist> GetQuery();
    Task<Artist?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Artist?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<List<Artist>> GetAsync(CancellationToken cancellationToken);
    Task<Artist> CreateAsync(Artist artist, CancellationToken cancellationToken);
    Task<Artist> UpdateAsync(Artist artist, CancellationToken cancellationToken);
    Task DeleteAsync(Artist artist, CancellationToken cancellationToken);
}
```

**Kod — `src/EDM.Application/Interfaces/Persistence/IContentJobRepository.cs`:**
```csharp
using EDM.Domain.Entities;
using EDM.Domain.Enums;

namespace EDM.Application.Interfaces.Persistence;

public interface IContentJobRepository
{
    IQueryable<ContentJob> GetQuery();
    Task<ContentJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ContentJob>> GetByArtistIdAsync(Guid artistId, CancellationToken cancellationToken);
    Task<List<ContentJob>> GetByStatusAsync(ContentJobStatus status, CancellationToken cancellationToken);
    Task<ContentJob> CreateAsync(ContentJob job, CancellationToken cancellationToken);
    Task<ContentJob> UpdateAsync(ContentJob job, CancellationToken cancellationToken);
    Task DeleteAsync(ContentJob job, CancellationToken cancellationToken);
}
```

**Kod — `src/EDM.Persistence/Repositories/ArtistRepository.cs`:**
```csharp
using EDM.Application.Interfaces.Persistence;
using EDM.Domain.Entities;
using EDM.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EDM.Persistence.Repositories;

public class ArtistRepository : IArtistRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ArtistRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<Artist> GetQuery()
        => _dbContext.Artists.AsQueryable().AsNoTracking();

    public async Task<Artist?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _dbContext.Artists
            .Where(a => a.Id == id && a.IsActive)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Artist?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        => await _dbContext.Artists
            .Where(a => a.Slug == slug && a.IsActive)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Artist>> GetAsync(CancellationToken cancellationToken)
        => await _dbContext.Artists
            .Where(a => a.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<Artist> CreateAsync(Artist artist, CancellationToken cancellationToken)
    {
        await _dbContext.Artists.AddAsync(artist, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return artist;
    }

    public async Task<Artist> UpdateAsync(Artist artist, CancellationToken cancellationToken)
    {
        _dbContext.Artists.Update(artist);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return artist;
    }

    public async Task DeleteAsync(Artist artist, CancellationToken cancellationToken)
    {
        _dbContext.Artists.Remove(artist);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

**NOT:** `ContentJobRepository` aynı pattern'ı izler; `GetByArtistIdAsync` ve `GetByStatusAsync` metotlarında ilgili `.Where()` filtrelerini uygula. Captions navigation property'sini include etmek için `.Include(j => j.Captions)` eklenebilir.

**Bağımlılıklar:** TASK-003

**Tahmini Süre:** 1.5 saat

**Kabul Kriterleri:**
- 2 interface dosyası `EDM.Application` projesinde mevcuttur
- 2 implementasyon dosyası `EDM.Persistence` projesinde mevcuttur
- Her repository `ApplicationDbContext` constructor injection alır
- `Registration.cs` içinde `AddScoped` ile kayıtlıdır
- `dotnet build` hatasız geçer

---

### TASK-005 — İlk EF Core Migration

**Başlık:** InitialCreate migration'ını oluştur ve veritabanına uygula

**Açıklama:**  
`ApplicationDbContext` üzerinden `dotnet ef migrations add` komutuyla ilk migration'ı oluştur. Migration dosyası `EDM.Persistence` projesinde saklanır.

**Komutlar:**

```bash
# EDM.Persistence projesinde çalıştır (migration assembly burada)
dotnet ef migrations add InitialCreate \
  --project src/EDM.Persistence \
  --startup-project src/EDM.ApiHost \
  --output-dir Migrations

# Veritabanına uygula
dotnet ef database update \
  --project src/EDM.Persistence \
  --startup-project src/EDM.ApiHost
```

**Oluşacak dosyalar:**
```
src/EDM.Persistence/Migrations/
├── YYYYMMDDHHMMSS_InitialCreate.cs
├── YYYYMMDDHHMMSS_InitialCreate.Designer.cs
└── ApplicationDbContextModelSnapshot.cs
```

**Beklenen tablo yapısı:**
- `artists` tablosu: `id`, `name`, `slug`, `score`, `manual_override`, `last_score_update`, `youtube_views_90d`, `instagram_reach_90d`, `tik_tok_plays_90d`, `spotify_listeners`, `creation_time`, `updated_time`, `deletion_time`, `is_deleted`, `is_active`
- `content_jobs` tablosu: `id`, `artist_id` (FK), `year`, `event_name`, `raw_file_name`, `s3_key`, `status`, `scheduled_at`, `published_at`, `instagram_post_id`, `tik_tok_post_id`, `youtube_video_id`, `retry_count`, + audit alanları
- `captions` tablosu: `id`, `content_job_id` (FK), `platform`, `caption_text`, `hashtags`, `generated_at`, + audit alanları

**Bağımlılıklar:** TASK-003, TASK-004

**Tahmini Süre:** 0.5 saat

**Kabul Kriterleri:**
- Migration dosyaları `EDM.Persistence/Migrations/` altında oluşur
- `dotnet ef database update` komutu hata vermez
- PostgreSQL'de `edm_journal` veritabanında 3 tablo görünür (`artists`, `content_jobs`, `captions`)
- `__efcore_migrations` tablosu migration kaydını içerir

---

### TASK-006 — Application Katmanı: DTO'lar ve Servis Interface'leri

**Başlık:** Artist ve ContentJob için DTO sınıfları ve servis interface'leri oluştur

**Açıklama:**  
Template'deki `CampaignCreateDto`, `CampaignResponseDto`, `ICampaignService` yapısını model alarak EDM için karşılıklarını oluştur. Bu katman Controller ↔ Service arasındaki sözleşmeyi tanımlar.

**Dosyalar:**

```
src/EDM.Application/
├── Dtos/
│   ├── Artist/
│   │   ├── ArtistCreateDto.cs
│   │   ├── ArtistUpdateDto.cs
│   │   └── ArtistResponseDto.cs
│   └── ContentJob/
│       ├── ContentJobCreateDto.cs
│       ├── ContentJobUpdateDto.cs
│       └── ContentJobResponseDto.cs
└── Interfaces/
    └── Infrastructure/
        ├── IArtistService.cs
        └── IContentJobService.cs
```

**Kod — `ArtistCreateDto.cs`:**
```csharp
using System.ComponentModel.DataAnnotations;

namespace EDM.Application.Dtos.Artist;

public class ArtistCreateDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug sadece küçük harf, rakam ve tire içerebilir")]
    public string Slug { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Score { get; set; } = 50;

    public long YoutubeViews90d { get; set; }
    public long InstagramReach90d { get; set; }
    public long TikTokPlays90d { get; set; }
    public long SpotifyListeners { get; set; }
}
```

**Kod — `ArtistResponseDto.cs`:**
```csharp
namespace EDM.Application.Dtos.Artist;

public class ArtistResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool ManualOverride { get; set; }
    public DateTime LastScoreUpdate { get; set; }
    public long YoutubeViews90d { get; set; }
    public long InstagramReach90d { get; set; }
    public long TikTokPlays90d { get; set; }
    public long SpotifyListeners { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Kod — `IArtistService.cs`:**
```csharp
using EDM.Application.Dtos.Artist;

namespace EDM.Application.Interfaces.Infrastructure;

public interface IArtistService
{
    Task<List<ArtistResponseDto>> GetAsync(CancellationToken cancellationToken);
    Task<ArtistResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ArtistResponseDto> CreateAsync(ArtistCreateDto dto, CancellationToken cancellationToken);
    Task<ArtistResponseDto> UpdateAsync(Guid id, ArtistUpdateDto dto, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
```

**Kod — `ContentJobCreateDto.cs`:**
```csharp
using System.ComponentModel.DataAnnotations;

namespace EDM.Application.Dtos.ContentJob;

public class ContentJobCreateDto
{
    [Required]
    public Guid ArtistId { get; set; }

    [Required]
    [MaxLength(500)]
    public string RawFileName { get; set; } = string.Empty;

    [Range(2000, 2100)]
    public int Year { get; set; } = DateTime.UtcNow.Year;
}
```

**`IContentJobService.cs`** aynı pattern'ı izler; `GetByArtistIdAsync(Guid artistId, ...)` ve `UpdateStatusAsync(Guid id, ContentJobStatus status, ...)` metotlarını ekle.

**Bağımlılıklar:** TASK-002

**Tahmini Süre:** 1.5 saat

**Kabul Kriterleri:**
- 6 DTO dosyası ve 2 servis interface dosyası oluşturulur
- DTO'larda `[Required]`, `[MaxLength]`, `[Range]` validation attribute'ları mevcuttur
- `IArtistService` ve `IContentJobService` CRUD metodlarını içerir
- `dotnet build` hatasız geçer

---

### TASK-007 — Infrastructure Katmanı: Servis İmplementasyonları (CRUD)

**Başlık:** ArtistService ve ContentJobService implementasyonlarını yaz

**Açıklama:**  
`IArtistService` ve `IContentJobService` interface'lerini `EDM.Infrastructure` katmanında implemente et. Entity ↔ DTO mapping'i manuel olarak yap (AutoMapper Sprint 1'de zorunlu değil).

**Dosyalar:**

```
src/EDM.Infrastructure/
├── Services/
│   ├── ArtistService.cs       (YENİ)
│   └── ContentJobService.cs   (YENİ)
└── Configuration/
    └── Registration.cs        (YENİ — DI kayıt)
```

**Kod — `ArtistService.cs` (iskelet):**
```csharp
using EDM.Application.Dtos.Artist;
using EDM.Application.Interfaces.Infrastructure;
using EDM.Application.Interfaces.Persistence;
using EDM.Domain.Entities;

namespace EDM.Infrastructure.Services;

public class ArtistService : IArtistService
{
    private readonly IArtistRepository _artistRepository;

    public ArtistService(IArtistRepository artistRepository)
    {
        _artistRepository = artistRepository;
    }

    public async Task<List<ArtistResponseDto>> GetAsync(CancellationToken cancellationToken)
    {
        var artists = await _artistRepository.GetAsync(cancellationToken);
        return artists.Select(MapToDto).ToList();
    }

    public async Task<ArtistResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var artist = await _artistRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Artist {id} bulunamadı");
        return MapToDto(artist);
    }

    public async Task<ArtistResponseDto> CreateAsync(ArtistCreateDto dto, CancellationToken cancellationToken)
    {
        var artist = new Artist
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Slug = dto.Slug,
            Score = dto.Score,
            LastScoreUpdate = DateTime.UtcNow,
            YoutubeViews90d = dto.YoutubeViews90d,
            InstagramReach90d = dto.InstagramReach90d,
            TikTokPlays90d = dto.TikTokPlays90d,
            SpotifyListeners = dto.SpotifyListeners
        };
        var created = await _artistRepository.CreateAsync(artist, cancellationToken);
        return MapToDto(created);
    }

    public async Task<ArtistResponseDto> UpdateAsync(Guid id, ArtistUpdateDto dto, CancellationToken cancellationToken)
    {
        var artist = await _artistRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Artist {id} bulunamadı");
        // DTO'dan güncelle
        artist.Name = dto.Name ?? artist.Name;
        artist.Score = dto.Score ?? artist.Score;
        // ... diğer alanlar
        var updated = await _artistRepository.UpdateAsync(artist, cancellationToken);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var artist = await _artistRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Artist {id} bulunamadı");
        await _artistRepository.DeleteAsync(artist, cancellationToken);
    }

    private static ArtistResponseDto MapToDto(Artist a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        Slug = a.Slug,
        Score = a.Score,
        ManualOverride = a.ManualOverride,
        LastScoreUpdate = a.LastScoreUpdate,
        YoutubeViews90d = a.YoutubeViews90d,
        InstagramReach90d = a.InstagramReach90d,
        TikTokPlays90d = a.TikTokPlays90d,
        SpotifyListeners = a.SpotifyListeners,
        CreatedAt = a.CreationTime
    };
}
```

**`ContentJobService.cs`** aynı pattern'ı izler. Ek olarak `CreateAsync` içinde `FilenameParser` (TASK-008) servisini çağırarak `EventName` alanını doldurur.

**`Registration.cs`:**
```csharp
using EDM.Application.Interfaces.Infrastructure;
using EDM.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EDM.Infrastructure.Configuration;

public static class Registration
{
    public static IServiceCollection UseInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<IContentJobService, ContentJobService>();
        return services;
    }
}
```

**Bağımlılıklar:** TASK-004, TASK-006

**Tahmini Süre:** 2 saat

**Kabul Kriterleri:**
- `ArtistService` ve `ContentJobService` sınıfları tüm interface metotlarını implemente eder
- `KeyNotFoundException` bulunamayan kayıt durumunda fırlatılır
- Entity ↔ DTO dönüşümü `MapToDto` private metodu üzerinden yapılır
- `Registration.cs` DI kayıtlarını içerir
- `dotnet build` hatasız geçer

---

### TASK-008 — FilenameParser Servisi

**Başlık:** Dosya adından etkinlik adı ve yılı parse eden `FilenameParser` servisini yaz

**Açıklama:**  
EDM pipeline'ında Google Drive'dan gelen dosyalar belirli bir isimlendirme kuralına göre adlandırılır. Bu servis ham dosya adından `Year` ve `EventName` bilgisini çıkarır. Sprint 1'de desteklenen format: `{YYYY}_{EventName}.{ext}` (örn. `2024_Tomorrowland_Set.mp4`, `2023_ADE_Amsterdam.mov`).

**Dosyalar:**

```
src/EDM.Infrastructure/
└── Services/
    ├── FilenameParser.cs           (YENİ)
    └── FilenameParseResult.cs      (YENİ — result model)

src/EDM.Application/Interfaces/Infrastructure/
└── IFilenameParser.cs             (YENİ)
```

**Kod — `IFilenameParser.cs`:**
```csharp
namespace EDM.Application.Interfaces.Infrastructure;

public interface IFilenameParser
{
    FilenameParseResult Parse(string rawFileName);
}
```

**Kod — `FilenameParseResult.cs`:**
```csharp
namespace EDM.Infrastructure.Services;

public record FilenameParseResult(
    bool Success,
    int Year,
    string EventName,
    string? ErrorMessage = null
);
```

**Kod — `FilenameParser.cs`:**
```csharp
using System.Text.RegularExpressions;
using EDM.Application.Interfaces.Infrastructure;

namespace EDM.Infrastructure.Services;

public class FilenameParser : IFilenameParser
{
    // Desteklenen formatlar:
    // 2024_Tomorrowland_Set.mp4
    // 2023_ADE_Amsterdam_Main_Stage.mov
    // 2025_Ultra_Miami.mp4
    private static readonly Regex Pattern =
        new(@"^(\d{4})_(.+?)(\.[a-zA-Z0-9]+)?$", RegexOptions.Compiled);

    public FilenameParseResult Parse(string rawFileName)
    {
        if (string.IsNullOrWhiteSpace(rawFileName))
            return new FilenameParseResult(false, 0, string.Empty, "Dosya adı boş olamaz");

        var fileName = Path.GetFileNameWithoutExtension(rawFileName);
        var match = Pattern.Match(rawFileName);

        if (!match.Success)
            return new FilenameParseResult(false, 0, string.Empty,
                $"Desteklenmeyen format: '{rawFileName}'. Beklenen: YYYY_EventName.ext");

        if (!int.TryParse(match.Groups[1].Value, out var year) || year < 2000 || year > 2100)
            return new FilenameParseResult(false, 0, string.Empty,
                $"Geçersiz yıl: '{match.Groups[1].Value}'");

        var eventName = match.Groups[2].Value
            .Replace("_", " ")
            .Trim();

        return new FilenameParseResult(true, year, eventName);
    }
}
```

**Test senaryoları (kabul kriteri olarak):**

| Girdi | Beklenen Year | Beklenen EventName |
|-------|--------------|---------------------|
| `2024_Tomorrowland_Set.mp4` | 2024 | `Tomorrowland Set` |
| `2023_ADE_Amsterdam.mov` | 2023 | `ADE Amsterdam` |
| `invalid_file.mp4` | - | Success=false |
| `9999_Event.mp4` | - | Success=false |
| `` (boş) | - | Success=false |

**Bağımlılıklar:** TASK-006

**Tahmini Süre:** 1 saat

**Kabul Kriterleri:**
- `FilenameParser.Parse()` metodu yukarıdaki 5 test senaryosunun tümünü doğru işler
- Regex pattern hatalı formatta `Success=false` döner
- Servisi `IFilenameParser` üzerinden DI ile almak mümkündür (`Registration.cs`'e `AddScoped<IFilenameParser, FilenameParser>()` eklenir)
- Tüm hata durumları `ErrorMessage` alanında açıklayıcı mesaj içerir

---

### TASK-009 — SchedulingService: Prime Time Slot Hesaplama

**Başlık:** Platform bazında optimal yayın zamanı hesaplayan `SchedulingService`'i yaz

**Açıklama:**  
Artist skoruna ve platforma göre en uygun yayın saatini hesaplar. Yüksek skorlu artist'ler prime time slot'a, düşük skorlular off-peak'e planlanır. Sprint 1'de sabit slot tablosu kullanılır (ML entegrasyonu Sprint 2'de gelecek).

**Dosyalar:**

```
src/EDM.Infrastructure/Services/
└── SchedulingService.cs           (YENİ)

src/EDM.Application/Interfaces/Infrastructure/
└── ISchedulingService.cs          (YENİ)
```

**Kod — `ISchedulingService.cs`:**
```csharp
using EDM.Domain.Enums;

namespace EDM.Application.Interfaces.Infrastructure;

public interface ISchedulingService
{
    DateTime CalculatePrimeTimeSlot(int artistScore, Platform platform, DateTime baseDate);
}
```

**Kod — `SchedulingService.cs`:**
```csharp
using EDM.Application.Interfaces.Infrastructure;
using EDM.Domain.Enums;

namespace EDM.Infrastructure.Services;

public class SchedulingService : ISchedulingService
{
    // Prime time slot tanımları (UTC)
    // Kaynak: SocialBee & Hootsuite 2024 raporları
    private static readonly Dictionary<Platform, (TimeSpan PrimeStart, TimeSpan PrimeEnd, TimeSpan OffPeakStart)>
        PlatformSlots = new()
        {
            [Platform.Instagram] = (new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0)),
            [Platform.TikTok]    = (new TimeSpan(19, 0, 0), new TimeSpan(21, 0, 0), new TimeSpan(14, 0, 0)),
            [Platform.YouTube]   = (new TimeSpan(15, 0, 0), new TimeSpan(17, 0, 0), new TimeSpan(10, 0, 0)),
        };

    // Score >= 70 → prime time, 40-69 → orta slot, < 40 → off-peak
    public DateTime CalculatePrimeTimeSlot(int artistScore, Platform platform, DateTime baseDate)
    {
        var slots = PlatformSlots[platform];
        var targetDate = baseDate.Date; // sadece tarih kısmı

        TimeSpan selectedTime;

        if (artistScore >= 70)
        {
            selectedTime = slots.PrimeStart;
        }
        else if (artistScore >= 40)
        {
            // Prime start ile prime end arasının ortası
            selectedTime = slots.PrimeStart + TimeSpan.FromMinutes(
                (slots.PrimeEnd - slots.PrimeStart).TotalMinutes / 2);
        }
        else
        {
            selectedTime = slots.OffPeakStart;
        }

        return DateTime.SpecifyKind(targetDate + selectedTime, DateTimeKind.Utc);
    }
}
```

**Test senaryoları:**

| Score | Platform | Beklenen Saat (UTC) |
|-------|----------|----------------------|
| 85 | Instagram | 08:00 |
| 55 | Instagram | 09:00 |
| 30 | Instagram | 12:00 |
| 90 | TikTok | 19:00 |
| 90 | YouTube | 15:00 |

**Bağımlılıklar:** TASK-006

**Tahmini Süre:** 1 saat

**Kabul Kriterleri:**
- `CalculatePrimeTimeSlot` yukarıdaki 5 senaryoyu doğru hesaplar
- Dönen `DateTime` her zaman `DateTimeKind.Utc` türündedir
- Score sınır değerleri (70 ve 40) doğru çalışır
- `Registration.cs`'e `AddScoped<ISchedulingService, SchedulingService>()` eklenir

---

### TASK-010 — API Controller'ları: Artists ve ContentJobs

**Başlık:** ArtistController ve ContentJobController endpoint'lerini yaz

**Açıklama:**  
Template'deki `CampaignController.cs` yapısını model alarak RESTful endpoint'ler oluştur. Sprint 1'de `[AllowAnonymous]` kullanılır (auth Sprint 2). `ExceptionCatcherMiddleware` global hata yönetimini sağlar.

**Dosyalar:**

```
src/EDM.ApiHost/
├── Controllers/
│   ├── BaseController.cs         (template'den kopyala, namespace güncelle)
│   ├── ArtistController.cs       (YENİ)
│   └── ContentJobController.cs   (YENİ)
└── Middleware/
    └── ExceptionCatcherMiddleware.cs  (template'den kopyala, şirkete özel bağımlılıkları temizle)
```

**Kod — `ArtistController.cs`:**
```csharp
using EDM.Application.Dtos.Artist;
using EDM.Application.Interfaces.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDM.ApiHost.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class ArtistController : ControllerBase
{
    private readonly IArtistService _artistService;

    public ArtistController(IArtistService artistService)
    {
        _artistService = artistService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ArtistResponseDto>), 200)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await _artistService.GetAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ArtistResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _artistService.GetByIdAsync(id, cancellationToken));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ArtistResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] ArtistCreateDto dto, CancellationToken cancellationToken)
    {
        var created = await _artistService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ArtistResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ArtistUpdateDto dto, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _artistService.UpdateAsync(id, dto, cancellationToken));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _artistService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
```

**ContentJobController endpoint'leri:**

| Method | Route | Açıklama |
|--------|-------|---------|
| GET | `/api/contentjob` | Tüm job'ları listele |
| GET | `/api/contentjob/{id}` | ID ile getir |
| GET | `/api/contentjob/artist/{artistId}` | Artist'e göre filtrele |
| POST | `/api/contentjob` | Yeni job oluştur (FilenameParser çağrılır) |
| PUT | `/api/contentjob/{id}` | Güncelle |
| PATCH | `/api/contentjob/{id}/status` | Sadece status güncelle |
| DELETE | `/api/contentjob/{id}` | Sil |

**`Program.cs` güncellemesi** — şirkete özel middleware'leri kaldır, asgari konfigürasyon:
```csharp
builder.Services
    .UsePersistence(builder.Configuration)
    .UseInfrastructure();

app.UseMiddleware<ExceptionCatcherMiddleware>();
app.MapControllers();
```

**Bağımlılıklar:** TASK-007, TASK-008, TASK-009

**Tahmini Süre:** 2 saat

**Kabul Kriterleri:**
- `GET /api/artist` 200 döner ve boş liste veya kayıtlı artist'leri verir
- `POST /api/artist` geçerli DTO ile 201 döner ve yeni kayıt veritabanına yazılır
- `GET /api/artist/{gecersizId}` 404 döner
- `PATCH /api/contentjob/{id}/status` yalnızca `Status` alanını günceller
- Swagger UI (`/swagger`) tüm endpoint'leri listeler
- Geçersiz model state 400 döner

---

## Görev Bağımlılık Grafiği

```
TASK-001 (Şablon Kurulum)
    └── TASK-002 (Domain Entities)
            ├── TASK-003 (DbContext)
            │       └── TASK-004 (Repositories)
            │               └── TASK-005 (Migration) ✓ DB hazır
            └── TASK-006 (DTOs + Interfaces)
                    ├── TASK-007 (Servis Implementasyonları)
                    ├── TASK-008 (FilenameParser)
                    └── TASK-009 (SchedulingService)
                            └── TASK-010 (Controllers) ✓ Sprint 1 tamamlandı
```

**Paralel yürütme fırsatı:** TASK-008 ve TASK-009, TASK-007 ile paralel olarak geliştirilebilir (bağımsız servisler).

---

## Tamamlanan

*(Sprint 1 başlangıcında boş)*

---

## Sprint 1 Genel NuGet Bağımlılıkları

`EDM.Persistence` projesi:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.*" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.*" />
<PackageReference Include="EFCore.NamingConventions" Version="8.*" />
```

`EDM.ApiHost` projesi:
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.*" />
```

---

## Sprint 1 Dışı (Sprint 2+)

- Gmail → Google Drive entegrasyonu
- S3 yükleme pipeline'ı
- Claude AI caption üretimi
- Instagram / TikTok / YouTube yayın API'leri
- Background job scheduler (Hangfire veya Quartz)
- JWT Authentication
- Artist skor hesaplama algoritması
- AutoMapper entegrasyonu

---

# Sprint 2 Görev Listesi — EDM Journal Automation Platform
**Tarih:** 2026-07-27
**Sprint Hedefi:** Gmail entegrasyonu, Google Drive → S3 pipeline, Anthropic caption üretimi, Hangfire background jobs, platform yayın stub'ları

## Bekleyen

- [ ] **TASK-011** [YÜKSEK] IGmailService + GmailService — Gmail API entegrasyonu, yeni mailleri oku, Drive linkini ve dosya adını çıkar
  - Dosya: EDM.Application/Interfaces/Infrastructure/IGmailService.cs (YENİ)
  - Dosya: EDM.Application/Models/GmailMessage.cs (YENİ)
  - Dosya: EDM.Infrastructure/Services/GmailService.cs (YENİ)
  - NuGet: Google.Apis.Gmail.v1
  - appsettings: Gmail section (ClientId, ClientSecret, RefreshToken, UserId)

- [ ] **TASK-012** [YÜKSEK] IGoogleDriveService + GoogleDriveService — Drive linkinden fileId parse et, dosyayı stream olarak indir
  - Dosya: EDM.Application/Interfaces/Infrastructure/IGoogleDriveService.cs (YENİ)
  - Dosya: EDM.Infrastructure/Services/GoogleDriveService.cs (YENİ)
  - NuGet: Google.Apis.Drive.v3
  - appsettings: GoogleDrive section

- [ ] **TASK-013** [YÜKSEK] IS3Service + S3Service — Stream'i S3'e yükle, S3Key: videos/{year}/{eventName}/{artistSlug}.mp4
  - Dosya: EDM.Application/Interfaces/Infrastructure/IS3Service.cs (YENİ)
  - Dosya: EDM.Infrastructure/Services/S3Service.cs (YENİ)
  - NuGet: AWSSDK.S3
  - appsettings: Aws section (AccessKey, SecretKey, Region, BucketName)

- [ ] **TASK-014** [YÜKSEK] ICaptionService + CaptionService — Anthropic Claude API ile caption üret (HTTP client, SDK değil)
  - Dosya: EDM.Application/Interfaces/Infrastructure/ICaptionService.cs (YENİ)
  - Dosya: EDM.Application/Models/CaptionRequest.cs (YENİ)
  - Dosya: EDM.Infrastructure/Services/CaptionService.cs (YENİ)
  - Platform formatları: Instagram (🎧, 25-30 hashtag), TikTok (🔥, 5-8 hashtag), YouTube (Full Set başlık + uzun açıklama + 10-15 hashtag)
  - appsettings: Anthropic section (ApiKey, Model: claude-opus-4-5)
  - GenerateAllPlatformsAsync: 3 platform paralel üretim

- [ ] **TASK-015** [YÜKSEK] IBackgroundJobService + BackgroundJobService — Hangfire ile ContentJob pipeline'ını kuyruğa al
  - Dosya: EDM.Application/Interfaces/Infrastructure/IBackgroundJobService.cs (YENİ)
  - Dosya: EDM.Infrastructure/Services/BackgroundJobService.cs (YENİ)
  - NuGet: Hangfire.AspNetCore, Hangfire.PostgreSql
  - Program.cs: AddHangfire + UseHangfireDashboard(/hangfire)
  - appsettings: Hangfire section

- [ ] **TASK-016** [YÜKSEK] IContentPipelineService + ContentPipelineService — Pipeline orchestrator
  - Dosya: EDM.Application/Interfaces/Infrastructure/IContentPipelineService.cs (YENİ)
  - Dosya: EDM.Infrastructure/Services/ContentPipelineService.cs (YENİ)
  - Dosya: EDM.Application/Interfaces/Persistence/ICaptionRepository.cs (YENİ)
  - Dosya: EDM.Persistence/Repositories/CaptionRepository.cs (YENİ)
  - Adımlar: Pending→Processing→S3Upload→CaptionGenerate→CaptionGenerated→Scheduled

- [ ] **TASK-017** [ORTA] Platform Stub Servisleri — IInstagramService, ITikTokService, IYouTubeService + stub implementasyonlar
  - Dosyalar: 3 interface + 3 stub implementasyon
  - Stub: ILogger ile log yaz, rastgele PostId üret, Status=Published yap

- [ ] **TASK-018** [ORTA] Infrastructure Registration güncellemesi — tüm yeni servisleri DI'a kaydet
  - Dosya: EDM.Infrastructure/Configuration/Registration.cs (GÜNCELLE)
  - Dosya: EDM.Persistence/Configuration/Registration.cs (GÜNCELLE — CaptionRepository ekle)

- [ ] **TASK-019** [ORTA] appsettings.json güncellemesi — Gmail, GoogleDrive, Aws, Anthropic, Hangfire section'ları
  - Dosya: EDM.ApiHost/appsettings.json (GÜNCELLE)
  - Tüm değerler placeholder (YOUR_API_KEY vb.)

- [ ] **TASK-020** [DÜŞÜK] CHANGELOG.md Sprint 2 kaydı
  - Dosya: C:\Users\Kullanıcı\Source\journal-edm\CHANGELOG.md

## Tamamlanan

*(Sprint 2 başlangıcında boş)*

---

## Sprint 2 Detaylı Görev Notları

### TASK-011 Detay — GmailService

**IGmailService interface:**
```csharp
namespace EDM.Application.Interfaces.Infrastructure;

public interface IGmailService
{
    Task<List<GmailMessage>> GetNewMessagesAsync(CancellationToken cancellationToken);
    Task MarkAsReadAsync(string messageId, CancellationToken cancellationToken);
}
```

**GmailMessage DTO** (`EDM.Application/Models/GmailMessage.cs`):
```csharp
namespace EDM.Application.Models;

public record GmailMessage(
    string MessageId,
    string Subject,
    string Body,
    string? DriveLink,
    string? FileName,
    DateTime ReceivedAt
);
```

**GmailService implementasyon notları:**
- `Google.Apis.Gmail.v1` NuGet paketi kullan
- `appsettings.json`'dan `Gmail:ClientId`, `Gmail:ClientSecret`, `Gmail:RefreshToken`, `Gmail:UserId` oku
- `GoogleWebAuthorizationBroker` yerine `ClientSecrets` + `UserCredential` oluştur (headless)
- Gelen kutusunda `is:unread` subject'li mailler getir
- Body'den Google Drive linki (regex: `https://drive\.google\.com/[^\s"]+`) çıkar
- Subject'ten dosya adını çıkar (`.mp4` veya `.mov` uzantılı kelime varsa)
- `MarkAsReadAsync`: Gmail API ile `UNREAD` label'ı kaldır

---

### TASK-012 Detay — GoogleDriveService

**Interface:**
```csharp
namespace EDM.Application.Interfaces.Infrastructure;

public interface IGoogleDriveService
{
    Task<Stream> DownloadFileAsync(string driveLink, CancellationToken cancellationToken);
    string ParseFileId(string driveLink);
}
```

**GoogleDriveService implementasyon notları:**
- `Google.Apis.Drive.v3` NuGet paketi kullan
- `ParseFileId`: Drive linkinden `fileId` çıkar. Regex: `/d/([a-zA-Z0-9_-]+)` veya `id=([a-zA-Z0-9_-]+)`
- `DownloadFileAsync`: `driveService.Files.Get(fileId)` ile stream döndür

---

### TASK-013 Detay — S3Service

**Interface:**
```csharp
namespace EDM.Application.Interfaces.Infrastructure;

public interface IS3Service
{
    Task<string> UploadAsync(Stream fileStream, string s3Key, CancellationToken cancellationToken);
}
```

**S3Service implementasyon notları:**
- `AWSSDK.S3` NuGet paketi kullan
- `AmazonS3Client` oluştur, `TransferUtility` ile stream'i yükle
- S3Key formatı: `videos/{year}/{eventName}/{artistSlug}.mp4`
- Upload başarılıysa `s3Key` döndür

---

### TASK-014 Detay — CaptionService (Anthropic Claude API)

**ICaptionService interface:**
```csharp
using EDM.Domain.Entities;
using EDM.Domain.Enums;

namespace EDM.Application.Interfaces.Infrastructure;

public interface ICaptionService
{
    Task<Caption> GenerateAsync(string artistName, string eventName, int year, Platform platform, CancellationToken cancellationToken);
    Task<List<Caption>> GenerateAllPlatformsAsync(string artistName, string eventName, int year, Guid contentJobId, CancellationToken cancellationToken);
}
```

**CaptionService implementasyon notları:**
- HTTP client kullan (SDK değil), endpoint: `https://api.anthropic.com/v1/messages`
- Header'lar: `x-api-key: {ApiKey}`, `anthropic-version: 2023-06-01`, `Content-Type: application/json`
- `GenerateAllPlatformsAsync`: 3 platform için paralel olarak çağır
- CaptionText'ten hashtag'leri ayrı `Hashtags` alanına yaz

**Platform prompt formatları:**

Instagram: `{Sanatçı} @ {Etkinlik} {Yıl} 🎧` + 2-3 cümle açıklama + 25-30 hashtag (zorunlu: #{eventName}, #{eventName}{year}, #{artistName}, #EDM, #electronicmusic, #djset, #edmjournal, #rave, #festival)

TikTok: tek satır — `{Sanatçı} @ {Etkinlik} {Yıl} 🔥` + 5-8 hashtag (zorunlu: #{artistName}, #{eventName}, #EDM, #djset, #fyp)

YouTube: `{Sanatçı} @ {Etkinlik} {Yıl} | Full Set` başlık + 3-4 cümle açıklama + timestamps placeholder + 10-15 hashtag (zorunlu: #{artistName}, #{eventName}, #{eventName}{year}, #EDM, #fullset, #djset, #electronicmusic, #rave, #festival, #edmjournal)

---

### TASK-015 Detay — BackgroundJobService (Hangfire)

**Interface:**
```csharp
namespace EDM.Application.Interfaces.Infrastructure;

public interface IBackgroundJobService
{
    string EnqueueContentPipeline(Guid contentJobId);
    string SchedulePublication(Guid contentJobId, DateTime scheduledAt);
}
```

**Program.cs eklentileri:**
```csharp
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("Hangfire")));
builder.Services.AddHangfireServer();

app.UseHangfireDashboard("/hangfire");
```

---

### TASK-016 Detay — ContentPipelineService (Orchestrator)

**Interface:**
```csharp
namespace EDM.Application.Interfaces.Infrastructure;

public interface IContentPipelineService
{
    Task ExecuteAsync(Guid contentJobId, CancellationToken cancellationToken);
}
```

**Pipeline adımları:**
1. ContentJob'u yükle (Status: Pending → Processing)
2. Artist'i yükle
3. DriveLink'ten dosyayı indir (`IGoogleDriveService.DownloadFileAsync`)
4. S3Key oluştur: `videos/{year}/{eventName}/{artist.Slug}.mp4`
5. S3'e yükle (`IS3Service.UploadAsync`)
6. ContentJob.S3Key güncelle
7. 3 platform için caption üret (`ICaptionService.GenerateAllPlatformsAsync`)
8. Caption'ları veritabanına kaydet
9. Status: CaptionGenerated → Scheduled
10. ScheduledAt: `ISchedulingService.CalculatePrimeTimeSlot` ile hesapla (Instagram için)

**ICaptionRepository** (`EDM.Application/Interfaces/Persistence/ICaptionRepository.cs`):
```csharp
using EDM.Domain.Entities;

namespace EDM.Application.Interfaces.Persistence;

public interface ICaptionRepository
{
    Task<Caption> CreateAsync(Caption caption, CancellationToken cancellationToken);
    Task<List<Caption>> GetByContentJobIdAsync(Guid contentJobId, CancellationToken cancellationToken);
}
```

---

### TASK-017 Detay — Platform Stub Servisleri

**Interface metotları:**
```csharp
// IInstagramService
Task<string> PublishReelAsync(string s3Key, string caption, CancellationToken cancellationToken);

// ITikTokService
Task<string> PublishVideoAsync(string s3Key, string caption, CancellationToken cancellationToken);

// IYouTubeService
Task<string> UploadVideoAsync(string s3Key, string title, string description, CancellationToken cancellationToken);
```

**Stub implementasyon notları:**
- `ILogger<T>` inject et
- Log: `"[STUB] Instagram/TikTok/YouTube yayın simüle edildi: {s3Key}"`
- PostId: `$"stub_{Guid.NewGuid():N}"`
- ContentJob'a ilgili PostId yaz, Status'u Published'a güncelle

---

### TASK-018 Detay — Infrastructure Registration

`EDM.Infrastructure/Configuration/Registration.cs` eklemeleri:
```csharp
services.AddScoped<IGmailService, GmailService>();
services.AddScoped<IGoogleDriveService, GoogleDriveService>();
services.AddScoped<IS3Service, S3Service>();
services.AddScoped<ICaptionService, CaptionService>();
services.AddScoped<IBackgroundJobService, BackgroundJobService>();
services.AddScoped<IContentPipelineService, ContentPipelineService>();
services.AddScoped<IInstagramService, InstagramService>();
services.AddScoped<ITikTokService, TikTokService>();
services.AddScoped<IYouTubeService, YouTubeService>();
services.AddHttpClient<CaptionService>(); // Anthropic HTTP client
```

`EDM.Persistence/Configuration/Registration.cs` eklentisi:
```csharp
services.AddScoped<ICaptionRepository, CaptionRepository>();
```

---

### TASK-019 Detay — appsettings.json Section'ları

```json
"Gmail": {
  "ClientId": "YOUR_CLIENT_ID",
  "ClientSecret": "YOUR_CLIENT_SECRET",
  "RefreshToken": "YOUR_REFRESH_TOKEN",
  "UserId": "me"
},
"GoogleDrive": {
  "ClientId": "YOUR_CLIENT_ID",
  "ClientSecret": "YOUR_CLIENT_SECRET",
  "RefreshToken": "YOUR_REFRESH_TOKEN"
},
"Aws": {
  "AccessKey": "YOUR_ACCESS_KEY",
  "SecretKey": "YOUR_SECRET_KEY",
  "Region": "eu-west-1",
  "BucketName": "edm-journal-videos"
},
"Anthropic": {
  "ApiKey": "YOUR_ANTHROPIC_API_KEY",
  "Model": "claude-opus-4-5"
},
"Hangfire": {
  "ConnectionString": "Host=localhost;Port=5432;Database=edm_journal;Username=postgres;Password=your_password"
}
```

---

## Sprint 2 Görev Bağımlılık Grafiği — TAMAMLANDI

```
TASK-011 (GmailService)
    └── TASK-012 (GoogleDriveService)
            └── TASK-013 (S3Service)
                    └── TASK-016 (ContentPipelineService — Orchestrator)
                            ├── TASK-014 (CaptionService) ← paralel
                            └── TASK-017 (Platform Stubs) ← paralel

TASK-015 (BackgroundJobService/Hangfire) ← TASK-016 tetikler

TASK-018 (Registration) ← tüm servisler tamamlandıktan sonra
TASK-019 (appsettings.json) ← TASK-018 ile birlikte
TASK-020 (CHANGELOG.md) ← en son
```

**Paralel yürütme fırsatı:** TASK-011, TASK-012, TASK-013, TASK-014, TASK-015, TASK-017 birbirinden büyük ölçüde bağımsız — önce interface'ler yazılır, ardından implementasyonlar paralel geliştirilebilir.

---

# Sprint 3 Görev Listesi — EDM Journal Automation Platform
**Tarih:** 2026-07-27
**Sprint Hedefi:** Gerçek platform API entegrasyonları, artist skor motoru, Gmail otomatik tetikleyici, admin dashboard

## Tamamlanan

- [x] **TASK-021** [YÜKSEK] S3Service.GetPresignedUrl — platform servislerine video URL sağlar
- [x] **TASK-022** [YÜKSEK] InstagramService — Graph API v19.0 gerçek implementasyonu (container → poll → publish)
- [x] **TASK-023** [YÜKSEK] TikTokService — Content Posting API v2 (PULL_FROM_URL, status polling)
- [x] **TASK-024** [YÜKSEK] YouTubeService — Data API v3 OAuth2, resumable upload (S3→stream)
- [x] **TASK-025** [YÜKSEK] ContentPipelineService — 3 platform paralel yayın, gerçek servisler
- [x] **TASK-026** [YÜKSEK] IArtistScoreService + ArtistScoreService — ağırlıklı skor (YT35/IG30/TT25/Sp10)
- [x] **TASK-027** [YÜKSEK] GmailPollingJob — Hangfire recurring (*/5 * * * *), artist oluştur, ContentJob oluştur, pipeline tetikle
- [x] **TASK-028** [YÜKSEK] ArtistController — PATCH score override, POST recalculate, POST recalculate-all
- [x] **TASK-029** [ORTA] Program.cs — CORS + Hangfire recurring job kaydı
- [x] **TASK-030** [ORTA] appsettings.json — Instagram/TikTok/YouTube credential bölümleri
- [x] **TASK-031** [YÜKSEK] Next.js Admin Dashboard — /artists (skor tablosu+override), /artists/[id] (detay), /jobs (filtreli job izleme)
- [x] **TASK-032** [DÜŞÜK] CHANGELOG.md Sprint 3 kaydı

## Bekleyen (Sprint 4+)

- [ ] Artist metrik güncelleme API'leri (YouTube Analytics, Instagram Insights, TikTok API'den gerçek veriler)
- [ ] JWT Authentication (admin dashboard güvenliği)
- [ ] Vercel deploy (admin-dashboard)
- [ ] Retry mekanizması için Dead Letter Queue
- [ ] Webhook endpoint (Gmail push notification yerine polling yerine)
