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

export interface LyricSearchRequest {
  artist?: string;
  title?: string;
  language?: string;
}

export interface LyricResult {
  id: number;
  lyricsUrl?: string;
  artistUrl?: string;
  artist?: string;
  title?: string;
  lyric?: string;
}

export interface LyricSearchResults {
  lyricResults: LyricResult[];
}
