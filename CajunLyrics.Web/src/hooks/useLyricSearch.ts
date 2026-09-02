import { useState, useCallback } from 'react';
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
    setState({ status: 'loading', result: null, results: [], errorMessage: null });

    try {
      const results = await fetchLyricSearchResults(request);

      if (!results.length) {
        setState({ status: 'not_found', result: null, results: [], errorMessage: 'No matching lyrics found for that search.' });
        return;
      }

      if (results.length === 1) {
        const lyric = await fetchLyric({
          artist: request.artist ?? results[0].artist,
          title: request.title ?? results[0].title,
          language: request.language,
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
      const lyric = await fetchLyric({ artist: result.artist, title: result.title });
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

  const reset = useCallback(() => setState(INITIAL_STATE), []);

  return { ...state, search, searchResults, selectResult, reset };
}
