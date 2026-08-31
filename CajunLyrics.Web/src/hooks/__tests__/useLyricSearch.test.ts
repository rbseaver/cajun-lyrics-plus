import { act, renderHook, waitFor } from '@testing-library/react';
import { useLyricSearch } from '../useLyricSearch';

describe('useLyricSearch', () => {
  const mockResult = {
    id: 7,
    artist: 'Zachary Richard',
    title: 'Travailler C\'est Trop Dur',
    artistUrl: 'https://www.cajunlyrics.com/artist/zachary-richard',
    lyricsUrl: 'https://www.cajunlyrics.com/lyric/travailler-cest-trop-dur',
    lyric: 'Là on travaille\n\nEt ça va mieux',
  };

  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('updates to success with the fetched lyric result', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        status: 200,
        json: async () => mockResult,
      }),
    );

    const { result } = renderHook(() => useLyricSearch());

    await act(async () => {
      await result.current.search({ artist: 'Zachary Richard', title: "Travailler C'est Trop Dur" });
    });

    expect(result.current.status).toBe('success');
    expect(result.current.result).toEqual(mockResult);
    expect(result.current.errorMessage).toBeNull();
    expect(fetch).toHaveBeenCalledWith(
      "/api/Lyrics/lyric?artist=Zachary+Richard&title=Travailler+C%27est+Trop+Dur",
      { headers: { Accept: 'application/json' } },
    );
  });

  it('updates to not_found when the API returns a 404', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 404,
      }),
    );

    const { result } = renderHook(() => useLyricSearch());

    await act(async () => {
      await result.current.search({ artist: 'Unknown Artist', title: 'No Song' });
    });

    await waitFor(() => {
      expect(result.current.status).toBe('not_found');
    });

    expect(result.current.result).toBeNull();
    expect(result.current.errorMessage).toBe('No lyrics found for that artist and title.');
  });

  it('updates to error for an unexpected server error', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 503,
      }),
    );

    const { result } = renderHook(() => useLyricSearch());

    await act(async () => {
      await result.current.search({ artist: 'Zachary Richard' });
    });

    await waitFor(() => {
      expect(result.current.status).toBe('error');
    });

    expect(result.current.errorMessage).toBe('Unexpected error from server (503).');
  });
});
