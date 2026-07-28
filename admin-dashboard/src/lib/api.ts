const BASE = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000'

export interface Artist {
  id: string
  name: string
  slug: string
  score: number
  manualOverride: boolean
  lastScoreUpdate: string
  youtubeViews90d: number
  instagramReach90d: number
  tikTokPlays90d: number
  spotifyListeners: number
  createdAt: string
}

export interface ContentJob {
  id: string
  artistId: string
  artistName?: string
  year: number
  eventName: string
  rawFileName: string
  s3Key: string
  status: string
  scheduledAt: string | null
  publishedAt: string | null
  instagramPostId: string | null
  tikTokPostId: string | null
  youtubeVideoId: string | null
  retryCount: number
}

async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  })
  if (!res.ok) {
    const err = await res.text()
    throw new Error(`API ${res.status}: ${err}`)
  }
  return res.json() as Promise<T>
}

export const api = {
  // Artist
  getArtists: () => apiFetch<Artist[]>('/api/artist'),
  getArtist: (id: string) => apiFetch<Artist>(`/api/artist/${id}`),
  createArtist: (data: Partial<Artist>) =>
    apiFetch<Artist>('/api/artist', { method: 'POST', body: JSON.stringify(data) }),
  updateArtist: (id: string, data: Partial<Artist>) =>
    apiFetch<Artist>(`/api/artist/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  overrideScore: (id: string, score: number, clearManualOverride = false) =>
    apiFetch<Artist>(`/api/artist/${id}/score`, {
      method: 'PATCH',
      body: JSON.stringify({ score, clearManualOverride }),
    }),
  recalculateScore: (id: string) =>
    apiFetch<Artist>(`/api/artist/${id}/score/recalculate`, { method: 'POST' }),
  recalculateAllScores: () =>
    apiFetch<{ message: string }>('/api/artist/score/recalculate-all', { method: 'POST' }),

  // ContentJob
  getJobs: () => apiFetch<ContentJob[]>('/api/contentjob'),
  getJobsByArtist: (artistId: string) =>
    apiFetch<ContentJob[]>(`/api/contentjob/artist/${artistId}`),
}
