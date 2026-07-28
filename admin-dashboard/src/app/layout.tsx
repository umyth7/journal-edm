import type { Metadata } from 'next'

export const metadata: Metadata = {
  title: 'EDM Journal Admin',
  description: 'EDM Journal Automation Platform — Admin Dashboard',
}

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="tr">
      <body style={{ margin: 0, fontFamily: 'system-ui, sans-serif', background: '#0f0f0f', color: '#f0f0f0' }}>
        <nav style={{
          background: '#1a1a1a',
          borderBottom: '1px solid #333',
          padding: '12px 24px',
          display: 'flex',
          alignItems: 'center',
          gap: '24px'
        }}>
          <span style={{ fontWeight: 700, fontSize: '18px', color: '#a855f7' }}>EDM Journal</span>
          <a href="/artists" style={{ color: '#ccc', textDecoration: 'none' }}>Artistler</a>
          <a href="/jobs" style={{ color: '#ccc', textDecoration: 'none' }}>İçerik Jobları</a>
        </nav>
        <main style={{ padding: '24px' }}>
          {children}
        </main>
      </body>
    </html>
  )
}
