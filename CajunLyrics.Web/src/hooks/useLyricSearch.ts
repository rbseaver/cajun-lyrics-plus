import { useState, useCallback } from 'react';
import { fetchLyric, LyricsApiError } from '../api/lyricsApi';
import type { LyricResult, LyricSearchRequest } from '../types/lyrics';

type SearchStatus = 'idle' | 'loading' | 'success' | 'not_found' | 'error';

interface UseLyricSearchState {
  status: SearchStatus;
  result: LyricResult | null;
  errorMessage: string | null;
}

interface UseLyricSearchReturn extends UseLyricSearchState {
  search: (request: LyricSearchRequest) => Promise<void>;
  reset: () => void;
}

const INITIAL_STATE: UseLyricSearchState = {
  status: 'idle',
  result: null,
  errorMessage: null,
};

export function useLyricSearch(): UseLyricSearchReturn {
  const [state, setState] = useState<UseLyricSearchState>(INITIAL_STATE);

  const search = useCallback(async (request: LyricSearchRequest) => {
    setState({ status: 'loading', result: null, errorMessage: null });

    try {
      const result = await fetchLyric(request);
      setState({ status: 'success', result, errorMessage: null });
    } catch (err) {
      if (err instanceof LyricsApiError && err.status === 404) {
        setState({ status: 'not_found', result: null, errorMessage: err.message });
      } else {
        const message =
          err instanceof Error ? err.message : 'An unexpected error occurred.';
        setState({ status: 'error', result: null, errorMessage: message });
      }
    }
  }, []);

  const reset = useCallback(() => setState(INITIAL_STATE), []);

  return { ...state, search, reset };
}
