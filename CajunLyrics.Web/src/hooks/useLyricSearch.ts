import { useState, useCallback, useRef } from 'react';
import { fetchLyric, fetchLyricSearchResults, LyricsApiError } from '../api/lyricsApi';
import type { LyricResult, LyricSearchRequest } from '../types/lyrics';

type SearchStatus = 'idle' | 'loading' | 'success' | 'results' | 'not_found' | 'error';

interface UseLyricSearchState {
  status: SearchStatus;
  result: LyricResult | null;
  results: LyricResult[];
  errorMessage: string | null;
}

interface UseLyricSearchReturn extends UseLyricSearchState {
  search: (request: LyricSearchRequest) => Promise<void>;
  searchResults: (request: LyricSearchRequest) => Promise<void>;
  selectResult: (result: LyricResult) => Promise<void>;
  reset: () => void;
}

const INITIAL_STATE: UseLyricSearchState = {
  status: 'idle',
  result: null,
  results: [],
  errorMessage: null,
};

export function useLyricSearch(): UseLyricSearchReturn {
  const [state, setState] = useState<UseLyricSearchState>(INITIAL_STATE);
  const lastSearchRef = useRef<LyricSearchRequest | null>(null);

  const search = useCallback(async (request: LyricSearchRequest) => {
    setState({ status: 'loading', result: null, results: [], errorMessage: null });

    try {
      const result = await fetchLyric(request);
      setState({ status: 'success', result, results: [], errorMessage: null });
    } catch (err) {
      if (err instanceof LyricsApiError && err.status === 404) {
        setState({ status: 'not_found', result: null, results: [], errorMessage: err.message });
      } else {
        const message =
          err instanceof Error ? err.message : 'An unexpected error occurred.';
        setState({ status: 'error', result: null, results: [], errorMessage: message });
      }
    }
  }, []);

  const searchResults = useCallback(async (request: LyricSearchRequest) => {
    lastSearchRef.current = request;
    setState({ status: 'loading', result: null, results: [], errorMessage: null });

    try {
      const results = await fetchLyricSearchResults(request);

      if (!results.length) {
        setState({ status: 'not_found', result: null, results: [], errorMessage: 'No matching lyrics found for that search.' });
        return;
      }

      if (results.length === 1) {
        // Fetch by the song the list actually resolved, not by the typed text —
        // the query may be a near-miss, and the backend matches strictly so it
        // never serves a different song.
        const lyric = await fetchLyric({
          artist: results[0].artist ?? request.artist,
          title: results[0].title ?? request.title,
          languages: request.languages,
        });
        setState({ status: 'success', result: lyric, results: [], errorMessage: null });
        return;
      }

      setState({ status: 'results', result: null, results, errorMessage: null });
    } catch (err) {
      if (err instanceof LyricsApiError && err.status === 404) {
        setState({ status: 'not_found', result: null, results: [], errorMessage: err.message });
      } else {
        const message =
          err instanceof Error ? err.message : 'An unexpected error occurred.';
        setState({ status: 'error', result: null, results: [], errorMessage: message });
      }
    }
  }, []);

  const selectResult = useCallback(async (result: LyricResult) => {
    if (!result.artist || !result.title) {
      setState({ status: 'error', result: null, results: [], errorMessage: 'That result is missing the artist or title.' });
      return;
    }

    setState({ status: 'loading', result: null, results: [], errorMessage: null });

    try {
      const lyric = await fetchLyric({
        artist: result.artist,
        title: result.title,
        languages: lastSearchRef.current?.languages,
      });
      setState({ status: 'success', result: lyric, results: [], errorMessage: null });
    } catch (err) {
      if (err instanceof LyricsApiError && err.status === 404) {
        setState({ status: 'not_found', result: null, results: [], errorMessage: err.message });
      } else {
        const message =
          err instanceof Error ? err.message : 'An unexpected error occurred.';
        setState({ status: 'error', result: null, results: [], errorMessage: message });
      }
    }
  }, []);

  const reset = useCallback(() => {
    lastSearchRef.current = null;
    setState(INITIAL_STATE);
  }, []);

  return { ...state, search, searchResults, selectResult, reset };
}
