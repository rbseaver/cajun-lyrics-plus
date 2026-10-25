export interface LyricLanguage {
  code: string;
  displayName: string;
}

export const LYRIC_LANGUAGES: LyricLanguage[] = [
  { code: 'fr', displayName: 'French' },
  { code: 'fr_LA', displayName: 'Louisiana French' },
  { code: 'fr_QC', displayName: 'Québécois French' },
  { code: 'fr_AC', displayName: 'Acadian French' },
  { code: 'en', displayName: 'English' },
];

export const ANY_LANGUAGE = 'all';

export interface LyricSearchRequest {
  id?: number;
  artist?: string;
  title?: string;
  /**
   * Languages to search. When omitted, the API defaults to Louisiana,
   * Acadian, and Québécois French; ANY_LANGUAGE disables the filter.
   */
  languages?: string[];
}

export interface LyricResult {
  id: number;
  lyricsUrl?: string;
  artistUrl?: string;
  artist?: string;
  title?: string;
  lyric?: string;
  /** The language filter that matched this lyric; absent when unknown. */
  language?: LyricLanguage;
}

export interface LyricSearchResults {
  lyricResults: LyricResult[];
}
