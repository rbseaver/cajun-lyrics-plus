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
        // Fetch by the song ID the list resolved, not by its possibly fuzzy text.
        const lyric = await fetchLyric({
          id: results[0].id,
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
    if (!Number.isInteger(result.id) || result.id <= 0) {
      setState({ status: 'error', result: null, results: [], errorMessage: 'That result is missing a valid ID.' });
      return;
    }

    setState({ status: 'loading', result: null, results: [], errorMessage: null });

    try {
      const lyric = await fetchLyric({
        id: result.id,
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
