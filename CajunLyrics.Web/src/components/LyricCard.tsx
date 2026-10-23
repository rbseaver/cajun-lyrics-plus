import type { LyricResult } from '../types/lyrics';

interface LyricCardProps {
  result: LyricResult;
}

export function LyricCard({ result }: LyricCardProps) {
  const lines = result.lyric?.split('\n') ?? [];

  return (
    <article className="lyric-card" aria-label={`Lyrics for ${result.title}`}>
      <header className="lyric-card__header">
        <div className="lyric-card__meta">
          <h2 className="lyric-card__title">{result.title}</h2>
          {result.artistUrl ? (
            <a
              href={result.artistUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="lyric-card__artist-link"
              aria-label={`View ${result.artist} on CajunLyrics`}
            >
              {result.artist}
              <svg
                aria-hidden="true"
                xmlns="http://www.w3.org/2000/svg"
                width="14"
                height="14"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
              >
                <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6" />
                <polyline points="15 3 21 3 21 9" />
                <line x1="10" y1="14" x2="21" y2="3" />
              </svg>
            </a>
          ) : (
            <span className="lyric-card__artist">{result.artist}</span>
          )}
          {result.language && (
            <span className="lyric-card__language">
              <svg
                aria-hidden="true"
                xmlns="http://www.w3.org/2000/svg"
                width="12"
                height="12"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
              >
                <circle cx="12" cy="12" r="10" />
                <line x1="2" y1="12" x2="22" y2="12" />
                <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z" />
              </svg>
              {result.language.displayName}
            </span>
          )}
        </div>

        {result.lyricsUrl && (
          <a
            href={result.lyricsUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="lyric-card__source-link"
            aria-label="View on CajunLyrics.com"
          >
            View on CajunLyrics.com
          </a>
        )}
      </header>

      <div className="lyric-card__divider" aria-hidden="true" />

      <div className="lyric-card__lyrics" role="region" aria-label="Song lyrics">
        {lines.map((line, i) =>
          line.trim() === '' ? (
            <div key={i} className="lyric-card__stanza-break" aria-hidden="true" />
          ) : (
            <p key={i} className="lyric-card__line">
              {line}
            </p>
          ),
        )}
      </div>
    </article>
  );
}
