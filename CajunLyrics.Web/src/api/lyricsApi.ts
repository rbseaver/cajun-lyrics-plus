import type { LyricResult, LyricSearchRequest, LyricSearchResults } from '../types/lyrics';

const BASE_URL = '/api/Lyrics';

export class LyricsApiError extends Error {
  readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = 'LyricsApiError';
    this.status = status;
  }
}

function buildSearchParams(request: LyricSearchRequest): URLSearchParams {
  const params = new URLSearchParams();

  if (request.id !== undefined) params.set('id', String(request.id));
  if (request.artist) params.set('artist', request.artist);
  if (request.title) params.set('title', request.title);
  for (const language of request.languages ?? []) {
    params.append('language', language);
  }

  return params;
}

export async function fetchLyric(request: LyricSearchRequest): Promise<LyricResult> {
  const params = buildSearchParams(request);

  const response = await fetch(`${BASE_URL}/lyric?${params.toString()}`, {
    headers: { Accept: 'application/json' },
  });

  if (response.status === 404) {
    throw new LyricsApiError('No lyrics found for that artist and title.', 404);
  }

  if (!response.ok) {
    throw new LyricsApiError(
      `Unexpected error from server (${response.status}).`,
      response.status,
    );
  }

  return response.json() as Promise<LyricResult>;
}

export async function fetchLyricSearchResults(
  request: LyricSearchRequest,
): Promise<LyricResult[]> {
  const params = buildSearchParams(request);

  const response = await fetch(`${BASE_URL}/search?${params.toString()}`, {
    headers: { Accept: 'application/json' },
  });

  if (response.status === 404) {
    throw new LyricsApiError('No matching lyrics found for that search.', 404);
  }

  if (!response.ok) {
    throw new LyricsApiError(
      `Unexpected error from server (${response.status}).`,
      response.status,
    );
  }

  const payload = (await response.json()) as LyricSearchResults;
  return Array.isArray(payload.lyricResults) ? payload.lyricResults : [];
}
