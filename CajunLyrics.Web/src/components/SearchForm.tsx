import { useState, type FormEvent } from 'react';
import { LYRIC_LANGUAGES } from '../types/lyrics';
import type { LyricSearchRequest } from '../types/lyrics';

interface SearchFormProps {
  onSearch: (request: LyricSearchRequest) => void;
  isLoading: boolean;
}

export function SearchForm({ onSearch, isLoading }: SearchFormProps) {
  const [artist, setArtist] = useState('');
  const [title, setTitle] = useState('');
  const [language, setLanguage] = useState('');

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!artist.trim() && !title.trim()) return;
    onSearch({
      artist: artist.trim() || undefined,
      title: title.trim() || undefined,
      language: language || undefined,
    });
  }

  return (
    <form className="search-form" onSubmit={handleSubmit} aria-label="Lyric search form">
      <div className="search-fields">
        <div className="field-group">
          <label htmlFor="artist-input" className="field-label">
            Artist
          </label>
          <input
            id="artist-input"
            type="text"
            className="field-input"
            placeholder="e.g. Zachary Richard"
            value={artist}
            onChange={(e) => setArtist(e.target.value)}
            autoComplete="off"
          />
        </div>

        <div className="field-group">
          <label htmlFor="title-input" className="field-label">
            Song Title
          </label>
          <input
            id="title-input"
            type="text"
            className="field-input"
            placeholder="e.g. Travailler C'est Trop Dur"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            autoComplete="off"
          />
        </div>

        <div className="field-group field-group--narrow">
          <label htmlFor="language-select" className="field-label">
            Language
          </label>
          <select
            id="language-select"
            className="field-input field-select"
            value={language}
            onChange={(e) => setLanguage(e.target.value)}
          >
            <option value="">Any</option>
            {LYRIC_LANGUAGES.map((lang) => (
              <option key={lang.code} value={lang.code}>
                {lang.displayName}
              </option>
            ))}
          </select>
        </div>
      </div>

      <button
        id="search-button"
        type="submit"
        className="search-button"
        disabled={isLoading || (!artist.trim() && !title.trim())}
        aria-label="Search for lyrics"
      >
        {isLoading ? (
          <span className="button-loading">
            <span className="button-spinner" aria-hidden="true" />
            Searching…
          </span>
        ) : (
          <>
            <svg
              aria-hidden="true"
              xmlns="http://www.w3.org/2000/svg"
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <circle cx="11" cy="11" r="8" />
              <line x1="21" y1="21" x2="16.65" y2="16.65" />
            </svg>
            Search Lyrics
          </>
        )}
      </button>
    </form>
  );
}
