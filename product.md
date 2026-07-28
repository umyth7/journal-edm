---
name: product
description: EDM Journal Automation Platform ürün tanımı. Tüm agentlar bağlam için bu dosyayı okur.
---

# EDM Journal Automation Platform

## Ürün Özeti

EDM Journal, Instagram, TikTok ve YouTube'da faaliyet gösteren bir EDM müzik içerik hesabıdır. Bu platform, e-posta yoluyla iletilen DJ set / etkinlik videolarını otomatik olarak analiz eder, içerik üretir ve 3 platformda zamanlanmış şekilde yayınlar.

**Hedef**: Manuel iş yükünü sıfıra indirmek. Elle müdahale yalnızca sanatçı puan düzenlemesi ve istisnai içerik onayı için gereklidir.

---

## Otomatik İçerik Pipeline'ı

```
1. Gmail'den yeni e-posta alımı
      ↓
2. Google Drive linkini tespit et, dosyayı S3'e indir
      ↓
3. Dosya adını parse et → {yıl, etkinlik, sanatçı}
      ↓
4. Sanatçı puanını veritabanından sorgula
      ↓
5. Caption + hashtag seti üret (platform bazlı)
      ↓
6. Puana göre prime time slotu belirle
      ↓
7. Instagram, TikTok, YouTube'a zamanlanmış yayın gönder
```

---

## Dosya Adı Parse Kuralı

**Format**: `{YIL}_{ETKİNLİK}_{SANATÇI}`

| Örnek dosya adı | Yıl | Etkinlik | Sanatçı |
|---|---|---|---|
| `2026_Tomorrowland_r3hab` | 2026 | Tomorrowland | R3hab |
| `2025_Ultra_Martin_Garrix` | 2025 | Ultra | Martin Garrix |
| `2026_ADE_Amelie_Lens` | 2026 | ADE | Amelie Lens |

**Parse kuralları:**
- Ayraç: alt çizgi `_`
- İlk token daima 4 haneli yıl
- İkinci token etkinlik adı (çok kelimeli ise her kelime ayrı token: `Electric_Daisy`)
- Son token(lar) sanatçı adı
- Tüm tokenlar başlık harfiyle normalize edilir (`r3hab` → `R3hab`)

---

## İçerik Üretim Kuralları

### Caption Formatı

**Instagram:**
```
{Sanatçı} @ {Etkinlik} {Yıl} 🎧

{Kısa açıklama — sanatçının stili, set'in enerjisi}

{Hashtag bloğu — 20-30 etiket}
```

**TikTok:**
```
{Sanatçı} @ {Etkinlik} {Yıl} 🔥 {3-5 hashtag}
```

**YouTube:**
```
Başlık: {Sanatçı} @ {Etkinlik} {Yıl} | Full Set
Açıklama: {Uzun açıklama} + {Timestamps} + {Hashtag}
```

### Hashtag Seti (Otomatik Üretim)
- **Etkinlik etiketleri**: `#{etkinlik}`, `#{etkinlik}{yıl}` (örn. `#Tomorrowland`, `#Tomorrowland2026`)
- **Sanatçı etiketleri**: `#{sanatçı}` (örn. `#R3hab`)
- **Genel EDM etiketleri**: `#EDM`, `#electronicmusic`, `#djset`, `#edmjournal`, `#rave`, `#festival`
- Platform bazlı etiket sayısı: Instagram 25–30, TikTok 5–8, YouTube 10–15

---

## Sanatçı Puanlama Sistemi

Her sanatçı **1–100** arasında bir puana sahiptir. Bu puan paylaşım zamanlamasını belirler.

### Puan Kaynakları (Otomatik)
| Kaynak | Ağırlık |
|---|---|
| YouTube kanal görüntülenme (son 90 gün) | %35 |
| Instagram reels görüntülenme (son 90 gün) | %30 |
| TikTok video play sayısı (son 90 gün) | %25 |
| Spotify aylık dinleyici | %10 |

### Manuel Override
Admin panosu üzerinden sanatçı puanı elle düzenlenebilir. Manuel değer, otomatik hesabın üzerine yazar ve `manualOverride: true` olarak işaretlenir.

### Prime Time Sıralaması
| Puan Aralığı | Yayın Dilimi |
|---|---|
| 80–100 | 19:00 – 21:00 (prime time) |
| 50–79 | 21:00 – 23:00 |
| 20–49 | 12:00 – 18:00 |
| 1–19 | 08:00 – 12:00 |

Aynı dilimde birden fazla sanatçı varsa puana göre sıralanır, aralarına 45 dakika boşluk bırakılır.

---

## Mimari

### Katmanlar

```
┌─────────────────────────────────────────────┐
│         Admin Dashboard (Next.js)            │
│         Vercel üzerinde deploy               │
└────────────────────┬────────────────────────┘
                     │ REST API
┌────────────────────▼────────────────────────┐
│         Backend API (.NET Web API / C#)      │
│  - Mail okuma (Gmail API / IMAP)             │
│  - Google Drive entegrasyonu                 │
│  - İçerik üretim servisi                     │
│  - Zamanlama motoru                          │
│  - Platform yayın servisleri                 │
│  - Sanatçı puanlama motoru                   │
└──────┬──────────────────────┬───────────────┘
       │                      │
┌──────▼──────┐      ┌───────▼──────────────┐
│ PostgreSQL  │      │      AWS S3           │
│ (Sanatçılar│      │  (Medya dosyaları,    │
│  İçerikler │      │   işlenmiş videolar)  │
│  Loglar)   │      └──────────────────────┘
└────────────┘
```

### Teknoloji Seçimleri

| Katman | Teknoloji | Neden |
|---|---|---|
| Frontend | Next.js 14+ | Vercel native desteği |
| Deployment | Vercel | Kullanıcının mevcut altyapısı |
| Backend | .NET 8 Web API (C#) | Kullanıcı tercihi |
| Storage | AWS S3 | Kullanıcının mevcut altyapısı |
| Veritabanı | PostgreSQL | Vercel Postgres veya Supabase |
| Mail | Gmail API | Mevcut mail altyapısı |
| Drive | Google Drive API v3 | Dosya iletim kanalı |

### Dış Platform API'leri

| Platform | API | Kısıt |
|---|---|---|
| Instagram | Graph API (Meta) | Business hesabı gerekir |
| TikTok | Content Posting API | TikTok for Business |
| YouTube | Data API v3 | OAuth 2.0, günlük kota |

---

## Veri Modeli (Özet)

### Artist
```
Id, Name, Slug, Score, ManualOverride, LastScoreUpdate,
YoutubeViews90d, InstagramReach90d, TikTokPlays90d, SpotifyListeners
```

### ContentJob
```
Id, ArtistId, Year, EventName, RawFileName,
S3Key, Status, ScheduledAt, PublishedAt,
InstagramPostId, TikTokPostId, YoutubeVideoId
```

### Caption
```
Id, ContentJobId, Platform, CaptionText, Hashtags, GeneratedAt
```

---

## Kısıtlar ve Geliştirme Kuralları

1. **Backend .NET zorunlu** — iş mantığı, zamanlama, API entegrasyonları yalnızca .NET'te
2. **Altyapı Vercel + S3** — başka bulut servisi eklenmez
3. **Tam otomasyon** — elle müdahale yalnızca puan override ve istisnai içerik onayı
4. **Platform sınırları korunur** — her platformun günlük post limiti aşılmaz
5. **Hata dayanıklılığı** — yayın başarısız olursa yeniden deneme mekanizması (max 3 kez), ardından log ve bildirim
6. **GDPR / veri saklama** — ham videolar S3'te 90 gün saklanır, sonra silinir

---

## Başarı Kriterleri

- Yeni mail geldiğinde 5 dakika içinde içerik hazır ve zamanlanmış olmalı
- 3 platformda yayın başarı oranı > %95
- Sanatçı puanı güncelleme gecikmesi < 24 saat
- Admin panosundan puan override 1 tıkla yapılabilmeli
