import type { LyricResult } from '../types/lyrics';
import type { LyricSearchRequest } from '../types/lyrics';

const BASE_URL = '/api/Lyrics';

export class LyricsApiError extends Error {
  readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = 'LyricsApiError';
    this.status = status;
  }
}

export async function fetchLyric(request: LyricSearchRequest): Promise<LyricResult> {
  const params = new URLSearchParams();

  if (request.artist) params.set('artist', request.artist);
  if (request.title) params.set('title', request.title);
  if (request.language) params.set('language', request.language);

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
