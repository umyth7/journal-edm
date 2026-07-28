# CHANGELOG

## [2026-07-27] - Sprint 3: Gerçek Platform API'leri, Artist Score Engine, Gmail Polling, Admin Dashboard
- **Ne yapıldı:**
  - Instagram Graph API v19.0 gerçek implementasyonu (container oluştur → poll → publish)
  - TikTok Content Posting API v2 gerçek implementasyonu (PULL_FROM_URL, status polling)
  - YouTube Data API v3 gerçek implementasyonu (OAuth2, resumable upload, S3→stream)
  - S3Service: `GetPresignedUrl` eklendi (platform servisler için)
  - ContentPipelineService: stub yerine gerçek platform servisleri, 3 platform paralel yayın
  - ArtistScoreService: ağırlıklı skor hesaplama (YT%35 / IG%30 / TT%25 / Spotify%10)
  - GmailPollingJob: Hangfire recurring job, 5 dakikada bir Gmail yoklama, otomatik ContentJob oluşturma
  - ArtistController: `PATCH /api/artist/{id}/score` (manuel override), `POST /api/artist/{id}/score/recalculate`, `POST /api/artist/score/recalculate-all`
  - Program.cs: CORS (Next.js), Hangfire recurring job kaydı
  - appsettings.json: Instagram, TikTok, YouTube credential bölümleri eklendi
  - Next.js Admin Dashboard: `/artists` (skor tablosu + override), `/artists/[id]` (detay + job tarihi), `/jobs` (status filter + platform badge)
- **Değişen dosyalar:** 15 dosya güncellendi/oluşturuldu, Next.js frontend (9 dosya)
- **Neden:** Sprint 3 — gerçek platform API entegrasyonu ve admin yönetim paneli
- **Test durumu:** dotnet build 0 error

## [2026-07-27] - Sprint 2: Gmail, Drive→S3 Pipeline, Caption, Hangfire, Platform Stubs
- **Ne yapıldı:** Gmail entegrasyonu (GmailClientService), Google Drive stream indirme (GoogleDriveService), AWS S3 yükleme (S3Service), Anthropic Claude caption üretimi HTTP client ile (CaptionService — 3 platform, paralel), Hangfire background job scheduler (PostgreSQL backend), ContentPipelineService pipeline orchestratoru, platform yayın stub'ları (Instagram/TikTok/YouTube), ICaptionRepository + CaptionRepository
- **Değişen dosyalar:** 18 yeni .cs dosyası, 4 güncellenmiş dosya (.csproj x2, Registration.cs x2, Program.cs, appsettings.json)
- **Neden:** TASKS.md Sprint 2 — TASK-011 ila TASK-020
- **Test durumu:** dotnet build 0 hata
