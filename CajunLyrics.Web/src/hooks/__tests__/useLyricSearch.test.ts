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

  it('updates to error when the request throws a network-level exception', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new Error('Network unavailable')));

    const { result } = renderHook(() => useLyricSearch());

    await act(async () => {
      await result.current.search({ artist: 'Zachary Richard' });
    });

    await waitFor(() => {
      expect(result.current.status).toBe('error');
    });

    expect(result.current.errorMessage).toBe('Network unavailable');
  });

  it('repeats the language param for a multi-language search', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        status: 200,
        json: async () => ({
          lyricResults: [
            { id: 1, artist: 'Zachary Richard', title: 'Le Fou' },
            { id: 2, artist: 'Zachary Richard', title: 'Travailler C\'est Trop Dur' },
          ],
        }),
      }),
    );

    const { result } = renderHook(() => useLyricSearch());

    await act(async () => {
      await result.current.searchResults({
        artist: 'Zachary Richard',
        languages: ['fr_LA', 'en'],
      });
    });

    expect(result.current.status).toBe('results');
    expect(fetch).toHaveBeenCalledWith(
      '/api/Lyrics/search?artist=Zachary+Richard&language=fr_LA&language=en',
      { headers: { Accept: 'application/json' } },
    );
  });

  it('keeps the search languages when selecting a result', async () => {
    vi.stubGlobal(
      'fetch',
      vi
        .fn()
        .mockResolvedValueOnce({
          ok: true,
          status: 200,
          json: async () => ({
            lyricResults: [
              { id: 1, artist: 'Feufollet', title: 'Blues de Dix Ans' },
              { id: 2, artist: 'Feufollet', title: 'Other Song' },
            ],
          }),
        })
        .mockResolvedValueOnce({
          ok: true,
          status: 200,
          json: async () => ({
            id: 1,
            artist: 'Feufollet',
            title: 'Blues de Dix Ans',
            lyric: 'Dix ans, dix ans',
          }),
        }),
    );

    const { result } = renderHook(() => useLyricSearch());

    await act(async () => {
      await result.current.searchResults({ artist: 'Feufollet', languages: ['en'] });
    });
    await act(async () => {
      await result.current.selectResult({ id: 1, artist: 'Feufollet', title: 'Blues de Dix Ans' });
    });

    expect(result.current.status).toBe('success');
    expect(fetch).toHaveBeenNthCalledWith(
      2,
      '/api/Lyrics/lyric?artist=Feufollet&title=Blues+de+Dix+Ans&language=en',
      { headers: { Accept: 'application/json' } },
    );
  });

  it('fetches lyrics by the resolved song when the list returns a single result', async () => {
    vi.stubGlobal(
      'fetch',
      vi
        .fn()
        .mockResolvedValueOnce({
          ok: true,
          status: 200,
          json: async () => ({
            lyricResults: [{ id: 3728, artist: 'Fayo', title: 'Moonshine' }],
          }),
        })
        .mockResolvedValueOnce({
          ok: true,
          status: 200,
          json: async () => ({
            id: 3728,
            artist: 'Fayo',
            title: 'Moonshine',
            lyric: 'Quand le soleil shine',
          }),
        }),
    );

    const { result } = renderHook(() => useLyricSearch());

    await act(async () => {
      // The typed title is a near-miss; the list resolved the actual song.
      await result.current.searchResults({ artist: 'Fayo', title: 'Moonshyne' });
    });

    expect(result.current.status).toBe('success');
    expect(fetch).toHaveBeenNthCalledWith(
      2,
      '/api/Lyrics/lyric?artist=Fayo&title=Moonshine',
      { headers: { Accept: 'application/json' } },
    );
  });
});
