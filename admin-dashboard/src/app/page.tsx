export default function Home() {
  return (
    <div style={{ maxWidth: 600 }}>
      <h1 style={{ color: '#a855f7' }}>EDM Journal Admin Dashboard</h1>
      <p style={{ color: '#aaa' }}>Platform otomasyonu yönetim paneli.</p>
      <div style={{ display: 'flex', gap: 16, marginTop: 24 }}>
        <a href="/artists" style={cardStyle}>
          <div style={{ fontSize: 32 }}>🎧</div>
          <div style={{ fontWeight: 600, marginTop: 8 }}>Artistler</div>
          <div style={{ color: '#aaa', fontSize: 14, marginTop: 4 }}>Skor yönetimi ve override</div>
        </a>
        <a href="/jobs" style={cardStyle}>
          <div style={{ fontSize: 32 }}>📋</div>
          <div style={{ fontWeight: 600, marginTop: 8 }}>İçerik Jobları</div>
          <div style={{ color: '#aaa', fontSize: 14, marginTop: 4 }}>Pipeline durum takibi</div>
        </a>
      </div>
    </div>
  )
}

const cardStyle: React.CSSProperties = {
  display: 'block',
  background: '#1a1a1a',
  border: '1px solid #333',
  borderRadius: 12,
  padding: '20px 24px',
  textDecoration: 'none',
  color: '#f0f0f0',
  minWidth: 160,
  cursor: 'pointer',
  transition: 'border-color 0.2s',
}
