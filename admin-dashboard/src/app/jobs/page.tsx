'use client'

import { useEffect, useState } from 'react'
import { api, ContentJob } from '@/lib/api'

const STATUS_COLOR: Record<string, string> = {
  Pending: '#6b7280',
  Processing: '#3b82f6',
  CaptionGenerated: '#8b5cf6',
  Scheduled: '#f59e0b',
  Published: '#22c55e',
  Failed: '#ef4444',
}

const STATUS_ORDER = ['Failed', 'Processing', 'Scheduled', 'Pending', 'CaptionGenerated', 'Published']

export default function JobsPage() {
  const [jobs, setJobs] = useState<ContentJob[]>([])
  const [loading, setLoading] = useState(true)
  const [filter, setFilter] = useState<string>('all')

  useEffect(() => {
    api.getJobs()
      .then(data => setJobs(data.sort((a, b) =>
        STATUS_ORDER.indexOf(a.status) - STATUS_ORDER.indexOf(b.status)
      )))
      .finally(() => setLoading(false))
  }, [])

  const statuses = ['all', ...Array.from(new Set(jobs.map(j => j.status)))]
  const filtered = filter === 'all' ? jobs : jobs.filter(j => j.status === filter)

  const counts = jobs.reduce((acc, j) => {
    acc[j.status] = (acc[j.status] ?? 0) + 1
    return acc
  }, {} as Record<string, number>)

  if (loading) return <p style={{ color: '#aaa' }}>Yükleniyor...</p>

  return (
    <div>
      <h1 style={{ marginBottom: 24 }}>İçerik Jobları</h1>

      {/* Summary cards */}
      <div style={{ display: 'flex', gap: 12, marginBottom: 24, flexWrap: 'wrap' }}>
        {Object.entries(counts).map(([status, count]) => (
          <div key={status} style={{
            background: '#1a1a1a',
            border: `1px solid ${STATUS_COLOR[status] ?? '#333'}44`,
            borderRadius: 8,
            padding: '10px 16px',
            minWidth: 100,
          }}>
            <div style={{ color: STATUS_COLOR[status] ?? '#aaa', fontSize: 12, fontWeight: 600 }}>{status}</div>
            <div style={{ fontSize: 28, fontWeight: 800, marginTop: 4 }}>{count}</div>
          </div>
        ))}
      </div>

      {/* Filter */}
      <div style={{ display: 'flex', gap: 8, marginBottom: 20 }}>
        {statuses.map(s => (
          <button
            key={s}
            onClick={() => setFilter(s)}
            style={{
              padding: '4px 12px',
              borderRadius: 999,
              border: `1px solid ${s === filter ? (STATUS_COLOR[s] ?? '#a855f7') : '#333'}`,
              background: s === filter ? '#1a1a1a' : 'transparent',
              color: s === filter ? (STATUS_COLOR[s] ?? '#a855f7') : '#6b7280',
              cursor: 'pointer',
              fontSize: 13,
            }}
          >
            {s} {s !== 'all' && counts[s] ? `(${counts[s]})` : ''}
          </button>
        ))}
      </div>

      {/* Table */}
      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ borderBottom: '1px solid #333', textAlign: 'left', color: '#aaa', fontSize: 13 }}>
            <th style={th}>Artist</th>
            <th style={th}>Etkinlik</th>
            <th style={th}>Yıl</th>
            <th style={th}>Durum</th>
            <th style={th}>Zamanlandı</th>
            <th style={th}>Yayınlandı</th>
            <th style={th}>Platformlar</th>
            <th style={th}>Retry</th>
          </tr>
        </thead>
        <tbody>
          {filtered.map(job => (
            <tr key={job.id} style={{ borderBottom: '1px solid #1a1a1a' }}>
              <td style={td}>
                <a href={`/artists/${job.artistId}`} style={{ color: '#a855f7', textDecoration: 'none' }}>
                  {job.artistId.slice(0, 8)}...
                </a>
              </td>
              <td style={td}>{job.eventName}</td>
              <td style={td}>{job.year}</td>
              <td style={td}>
                <span style={{
                  padding: '2px 8px',
                  borderRadius: 999,
                  fontSize: 12,
                  fontWeight: 600,
                  background: (STATUS_COLOR[job.status] ?? '#333') + '22',
                  color: STATUS_COLOR[job.status] ?? '#aaa',
                  border: `1px solid ${STATUS_COLOR[job.status] ?? '#333'}44`,
                }}>
                  {job.status}
                </span>
              </td>
              <td style={{ ...td, color: '#6b7280', fontSize: 12 }}>
                {job.scheduledAt ? new Date(job.scheduledAt).toLocaleString('tr-TR') : '—'}
              </td>
              <td style={{ ...td, color: '#6b7280', fontSize: 12 }}>
                {job.publishedAt ? new Date(job.publishedAt).toLocaleString('tr-TR') : '—'}
              </td>
              <td style={td}>
                <div style={{ display: 'flex', gap: 4 }}>
                  {job.instagramPostId && <PlatformBadge label="IG" />}
                  {job.tikTokPostId && <PlatformBadge label="TT" />}
                  {job.youtubeVideoId && <PlatformBadge label="YT" color="#ef4444" />}
                </div>
              </td>
              <td style={{ ...td, color: job.retryCount > 0 ? '#ef4444' : '#6b7280', fontSize: 13 }}>
                {job.retryCount}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {filtered.length === 0 && (
        <p style={{ color: '#6b7280', textAlign: 'center', marginTop: 48 }}>Job bulunamadı.</p>
      )}
    </div>
  )
}

function PlatformBadge({ label, color = '#a855f7' }: { label: string; color?: string }) {
  return (
    <span style={{
      fontSize: 11,
      padding: '1px 6px',
      borderRadius: 4,
      background: color + '22',
      color,
      border: `1px solid ${color}44`,
      fontWeight: 700,
    }}>
      {label}
    </span>
  )
}

const th: React.CSSProperties = { padding: '8px 12px', fontWeight: 500 }
const td: React.CSSProperties = { padding: '12px 12px' }
