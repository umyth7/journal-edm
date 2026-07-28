'use client'

import { useEffect, useState } from 'react'
import { useParams } from 'next/navigation'
import { api, Artist, ContentJob } from '@/lib/api'

const JOB_STATUS_COLOR: Record<string, string> = {
  Pending: '#6b7280',
  Processing: '#3b82f6',
  CaptionGenerated: '#8b5cf6',
  Scheduled: '#f59e0b',
  Published: '#22c55e',
  Failed: '#ef4444',
}

export default function ArtistDetailPage() {
  const { id } = useParams<{ id: string }>()
  const [artist, setArtist] = useState<Artist | null>(null)
  const [jobs, setJobs] = useState<ContentJob[]>([])
  const [loading, setLoading] = useState(true)
  const [scoreInput, setScoreInput] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    Promise.all([api.getArtist(id), api.getJobsByArtist(id)])
      .then(([a, j]) => {
        setArtist(a)
        setScoreInput(String(a.score))
        setJobs(j)
      })
      .finally(() => setLoading(false))
  }, [id])

  const handleOverride = async () => {
    const score = parseInt(scoreInput)
    if (isNaN(score) || score < 1 || score > 100) return alert('Score 1-100 arasında olmalı')
    setSaving(true)
    try {
      const updated = await api.overrideScore(id, score)
      setArtist(updated)
    } catch (e) {
      alert(String(e))
    } finally {
      setSaving(false)
    }
  }

  const handleClearOverride = async () => {
    setSaving(true)
    try {
      const updated = await api.recalculateScore(id)
      setArtist(updated)
      setScoreInput(String(updated.score))
    } catch (e) {
      alert(String(e))
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <p style={{ color: '#aaa' }}>Yükleniyor...</p>
  if (!artist) return <p style={{ color: '#ef4444' }}>Artist bulunamadı</p>

  const scoreColor = artist.score >= 80 ? '#22c55e' : artist.score >= 50 ? '#f59e0b' : '#ef4444'

  return (
    <div style={{ maxWidth: 900 }}>
      <a href="/artists" style={{ color: '#a855f7', textDecoration: 'none', fontSize: 14 }}>← Artistler</a>

      <div style={{ display: 'flex', alignItems: 'flex-start', gap: 32, marginTop: 24 }}>
        {/* Artist bilgileri */}
        <div style={{ flex: 1, background: '#1a1a1a', border: '1px solid #333', borderRadius: 12, padding: 24 }}>
          <h1 style={{ margin: '0 0 4px 0' }}>{artist.name}</h1>
          <p style={{ color: '#6b7280', margin: '0 0 24px 0', fontSize: 14 }}>{artist.slug}</p>

          <div style={{ fontSize: 64, fontWeight: 800, color: scoreColor, lineHeight: 1 }}>{artist.score}</div>
          <div style={{ color: '#aaa', fontSize: 13, marginTop: 4 }}>
            {artist.manualOverride ? '🔒 Manuel Override' : '🤖 Otomatik Hesaplama'}
          </div>

          <div style={{ marginTop: 24, display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <MetricCard label="YouTube (90d)" value={fmtNum(artist.youtubeViews90d)} />
            <MetricCard label="Instagram (90d)" value={fmtNum(artist.instagramReach90d)} />
            <MetricCard label="TikTok (90d)" value={fmtNum(artist.tikTokPlays90d)} />
            <MetricCard label="Spotify Dinleyici" value={fmtNum(artist.spotifyListeners)} />
          </div>
        </div>

        {/* Score override paneli */}
        <div style={{ width: 260, background: '#1a1a1a', border: '1px solid #333', borderRadius: 12, padding: 24 }}>
          <h3 style={{ margin: '0 0 16px 0', fontSize: 15 }}>Skor Override</h3>

          <label style={{ fontSize: 13, color: '#aaa' }}>Yeni Skor (1-100)</label>
          <input
            type="number"
            min={1}
            max={100}
            value={scoreInput}
            onChange={e => setScoreInput(e.target.value)}
            style={{
              display: 'block',
              width: '100%',
              marginTop: 6,
              padding: '8px 10px',
              background: '#111',
              border: '1px solid #555',
              borderRadius: 6,
              color: '#f0f0f0',
              fontSize: 16,
              boxSizing: 'border-box',
            }}
          />

          <button
            onClick={handleOverride}
            disabled={saving}
            style={{ ...btnStyle, marginTop: 12, width: '100%', background: '#7c3aed' }}
          >
            {saving ? 'Kaydediliyor...' : 'Manuel Override Uygula'}
          </button>

          {artist.manualOverride && (
            <button
              onClick={handleClearOverride}
              disabled={saving}
              style={{ ...btnStyle, marginTop: 8, width: '100%', background: '#374151' }}
            >
              Override Kaldır & Yeniden Hesapla
            </button>
          )}

          <p style={{ color: '#6b7280', fontSize: 12, marginTop: 12, lineHeight: 1.5 }}>
            Manuel override aktifken otomatik skor hesabı devre dışı kalır. Override kaldırıldığında mevcut metriklerden yeniden hesaplanır.
          </p>
        </div>
      </div>

      {/* ContentJob tablosu */}
      <h2 style={{ marginTop: 40, marginBottom: 16 }}>İçerik Jobları ({jobs.length})</h2>
      {jobs.length === 0 ? (
        <p style={{ color: '#6b7280' }}>Bu artist için henüz job yok.</p>
      ) : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ borderBottom: '1px solid #333', textAlign: 'left', color: '#aaa', fontSize: 13 }}>
              <th style={thStyle}>Etkinlik</th>
              <th style={thStyle}>Yıl</th>
              <th style={thStyle}>Durum</th>
              <th style={thStyle}>Zamanlandı</th>
              <th style={thStyle}>Yayınlandı</th>
              <th style={thStyle}>Retry</th>
            </tr>
          </thead>
          <tbody>
            {jobs.map(job => (
              <tr key={job.id} style={{ borderBottom: '1px solid #1a1a1a' }}>
                <td style={tdStyle}>{job.eventName}</td>
                <td style={tdStyle}>{job.year}</td>
                <td style={tdStyle}>
                  <span style={{
                    padding: '2px 8px',
                    borderRadius: 999,
                    fontSize: 12,
                    fontWeight: 600,
                    background: (JOB_STATUS_COLOR[job.status] ?? '#333') + '22',
                    color: JOB_STATUS_COLOR[job.status] ?? '#aaa',
                    border: `1px solid ${JOB_STATUS_COLOR[job.status] ?? '#333'}44`,
                  }}>
                    {job.status}
                  </span>
                </td>
                <td style={{ ...tdStyle, color: '#6b7280', fontSize: 13 }}>
                  {job.scheduledAt ? new Date(job.scheduledAt).toLocaleString('tr-TR') : '—'}
                </td>
                <td style={{ ...tdStyle, color: '#6b7280', fontSize: 13 }}>
                  {job.publishedAt ? new Date(job.publishedAt).toLocaleString('tr-TR') : '—'}
                </td>
                <td style={{ ...tdStyle, color: job.retryCount > 0 ? '#ef4444' : '#6b7280', fontSize: 13 }}>
                  {job.retryCount}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div style={{ background: '#111', borderRadius: 8, padding: '10px 14px' }}>
      <div style={{ color: '#6b7280', fontSize: 12 }}>{label}</div>
      <div style={{ fontWeight: 700, fontSize: 18, marginTop: 2 }}>{value}</div>
    </div>
  )
}

const fmtNum = (n: number) =>
  n >= 1_000_000 ? `${(n / 1_000_000).toFixed(1)}M`
    : n >= 1_000 ? `${(n / 1_000).toFixed(0)}K`
      : String(n)

const btnStyle: React.CSSProperties = {
  color: '#fff',
  border: 'none',
  borderRadius: 6,
  padding: '10px 16px',
  fontSize: 13,
  cursor: 'pointer',
  fontWeight: 500,
}

const thStyle: React.CSSProperties = { padding: '8px 12px', fontWeight: 500 }
const tdStyle: React.CSSProperties = { padding: '12px 12px' }
