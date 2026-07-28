'use client'

import { useEffect, useState } from 'react'
import { api, Artist } from '@/lib/api'

const STATUS_COLORS: Record<string, string> = {
  true: '#f59e0b',
  false: '#6b7280',
}

const SCORE_COLOR = (score: number) =>
  score >= 80 ? '#22c55e' : score >= 50 ? '#f59e0b' : '#ef4444'

export default function ArtistsPage() {
  const [artists, setArtists] = useState<Artist[]>([])
  const [loading, setLoading] = useState(true)
  const [recalculating, setRecalculating] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = async () => {
    try {
      const data = await api.getArtists()
      setArtists(data.sort((a, b) => b.score - a.score))
    } catch (e) {
      setError(String(e))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const handleRecalculateAll = async () => {
    setRecalculating(true)
    try {
      await api.recalculateAllScores()
      await load()
    } catch (e) {
      alert(String(e))
    } finally {
      setRecalculating(false)
    }
  }

  if (loading) return <p style={{ color: '#aaa' }}>Yükleniyor...</p>
  if (error) return <p style={{ color: '#ef4444' }}>Hata: {error}</p>

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 24 }}>
        <h1 style={{ margin: 0 }}>Artistler</h1>
        <button
          onClick={handleRecalculateAll}
          disabled={recalculating}
          style={btnStyle('#7c3aed')}
        >
          {recalculating ? 'Hesaplanıyor...' : 'Tüm Skorları Yeniden Hesapla'}
        </button>
      </div>

      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ borderBottom: '1px solid #333', textAlign: 'left', color: '#aaa', fontSize: 13 }}>
            <th style={th}>Artist</th>
            <th style={th}>Slug</th>
            <th style={th}>Skor</th>
            <th style={th}>Manuel Override</th>
            <th style={th}>Son Güncelleme</th>
            <th style={th}>İşlem</th>
          </tr>
        </thead>
        <tbody>
          {artists.map(artist => (
            <ArtistRow key={artist.id} artist={artist} onUpdate={load} />
          ))}
        </tbody>
      </table>

      {artists.length === 0 && (
        <p style={{ color: '#aaa', textAlign: 'center', marginTop: 48 }}>Henüz artist yok.</p>
      )}
    </div>
  )
}

function ArtistRow({ artist, onUpdate }: { artist: Artist; onUpdate: () => void }) {
  const [editing, setEditing] = useState(false)
  const [scoreInput, setScoreInput] = useState(String(artist.score))
  const [saving, setSaving] = useState(false)

  const handleSave = async () => {
    const score = parseInt(scoreInput)
    if (isNaN(score) || score < 1 || score > 100) return alert('Score 1-100 arasında olmalı')
    setSaving(true)
    try {
      await api.overrideScore(artist.id, score)
      setEditing(false)
      onUpdate()
    } catch (e) {
      alert(String(e))
    } finally {
      setSaving(false)
    }
  }

  const handleClearOverride = async () => {
    setSaving(true)
    try {
      await api.recalculateScore(artist.id)
      onUpdate()
    } catch (e) {
      alert(String(e))
    } finally {
      setSaving(false)
    }
  }

  return (
    <tr style={{ borderBottom: '1px solid #222' }}>
      <td style={td}>
        <a href={`/artists/${artist.id}`} style={{ color: '#a855f7', textDecoration: 'none', fontWeight: 600 }}>
          {artist.name}
        </a>
      </td>
      <td style={{ ...td, color: '#6b7280', fontSize: 13 }}>{artist.slug}</td>
      <td style={td}>
        {editing ? (
          <input
            type="number"
            min={1}
            max={100}
            value={scoreInput}
            onChange={e => setScoreInput(e.target.value)}
            style={{ width: 60, background: '#111', color: '#f0f0f0', border: '1px solid #555', borderRadius: 4, padding: '2px 6px' }}
          />
        ) : (
          <span style={{
            fontWeight: 700,
            fontSize: 18,
            color: SCORE_COLOR(artist.score)
          }}>
            {artist.score}
          </span>
        )}
      </td>
      <td style={td}>
        <span style={{
          fontSize: 12,
          padding: '2px 8px',
          borderRadius: 999,
          background: artist.manualOverride ? '#451a03' : '#1c1c1c',
          color: artist.manualOverride ? '#f59e0b' : '#6b7280',
          border: `1px solid ${artist.manualOverride ? '#92400e' : '#333'}`
        }}>
          {artist.manualOverride ? 'Manuel' : 'Otomatik'}
        </span>
      </td>
      <td style={{ ...td, color: '#6b7280', fontSize: 13 }}>
        {new Date(artist.lastScoreUpdate).toLocaleDateString('tr-TR')}
      </td>
      <td style={td}>
        <div style={{ display: 'flex', gap: 8 }}>
          {editing ? (
            <>
              <button onClick={handleSave} disabled={saving} style={btnStyle('#16a34a', true)}>
                {saving ? '...' : 'Kaydet'}
              </button>
              <button onClick={() => setEditing(false)} style={btnStyle('#374151', true)}>İptal</button>
            </>
          ) : (
            <>
              <button onClick={() => setEditing(true)} style={btnStyle('#7c3aed', true)}>Skoru Düzenle</button>
              {artist.manualOverride && (
                <button onClick={handleClearOverride} disabled={saving} style={btnStyle('#374151', true)}>
                  {saving ? '...' : 'Override Kaldır'}
                </button>
              )}
            </>
          )}
        </div>
      </td>
    </tr>
  )
}

const th: React.CSSProperties = { padding: '8px 12px', fontWeight: 500 }
const td: React.CSSProperties = { padding: '12px 12px' }

const btnStyle = (bg: string, small = false): React.CSSProperties => ({
  background: bg,
  color: '#fff',
  border: 'none',
  borderRadius: 6,
  padding: small ? '4px 10px' : '8px 16px',
  fontSize: small ? 13 : 14,
  cursor: 'pointer',
  fontWeight: 500,
})
