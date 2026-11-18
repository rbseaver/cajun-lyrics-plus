import { useEffect, useLayoutEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { translateToEnglish } from '../api/translationApi';
import type { LyricResult } from '../types/lyrics';

interface LyricCardProps {
  result: LyricResult;
}

const MAX_TRANSLATION_LENGTH = 5000;
const POPOVER_GAP = 8;
const VIEWPORT_MARGIN = 12;

interface SelectionBounds {
  top: number;
  bottom: number;
  left: number;
}

export function LyricCard({ result }: LyricCardProps) {
  const lines = result.lyric?.split('\n') ?? [];
  const lyricsRef = useRef<HTMLDivElement>(null);
  const popoverRef = useRef<HTMLElement>(null);
  const [selectedText, setSelectedText] = useState('');
  const [selectionBounds, setSelectionBounds] = useState<SelectionBounds | null>(null);
  const [popoverPosition, setPopoverPosition] = useState({ left: 0, top: 0 });
  const [translation, setTranslation] = useState('');
  const [errorMessage, setErrorMessage] = useState('');
  const [isTranslating, setIsTranslating] = useState(false);
  const [isOpen, setIsOpen] = useState(false);

  useEffect(() => {
    function handleSelectionChange() {
      const selection = window.getSelection();
      const root = lyricsRef.current;
      const anchor = selection?.anchorNode;
      const focus = selection?.focusNode;
      const range = selection?.rangeCount ? selection.getRangeAt(0) : null;
      const text = selection?.toString().trim() ?? '';

      if (!root || !anchor || !focus || !range || !root.contains(anchor) || !root.contains(focus) || !text) {
        setSelectedText('');
        setSelectionBounds(null);
        setIsOpen(false);
        return;
      }

      const rect = range.getBoundingClientRect();
      setSelectedText(text);
      setSelectionBounds({ top: rect.top, bottom: rect.bottom, left: rect.left });
      setTranslation('');
      setErrorMessage('');
      setIsOpen(true);
    }

    document.addEventListener('selectionchange', handleSelectionChange);
    return () => document.removeEventListener('selectionchange', handleSelectionChange);
  }, []);

  useLayoutEffect(() => {
    const popover = popoverRef.current;
    if (!isOpen || !selectionBounds || !popover) return;

    const { width, height } = popover.getBoundingClientRect();
    const viewportWidth = document.documentElement.clientWidth || window.innerWidth;
    const viewportHeight = document.documentElement.clientHeight || window.innerHeight;
    const belowTop = selectionBounds.bottom + POPOVER_GAP;
    const aboveTop = selectionBounds.top - height - POPOVER_GAP;
    const top =
      belowTop + height <= viewportHeight - VIEWPORT_MARGIN
        ? belowTop
        : aboveTop >= VIEWPORT_MARGIN
          ? aboveTop
          : Math.max(VIEWPORT_MARGIN, viewportHeight - height - VIEWPORT_MARGIN);
    const left = Math.max(
      VIEWPORT_MARGIN,
      Math.min(selectionBounds.left, viewportWidth - width - VIEWPORT_MARGIN),
    );

    setPopoverPosition({ left, top });
  }, [errorMessage, isOpen, isTranslating, selectionBounds, translation]);

  async function handleTranslate() {
    setIsTranslating(true);
    setErrorMessage('');

    try {
      setTranslation(await translateToEnglish(selectedText));
    } catch {
      setErrorMessage('Translation is unavailable right now. Please try again.');
    } finally {
      setIsTranslating(false);
    }
  }

  function closeTranslation() {
    setIsOpen(false);
    setSelectedText('');
    setTranslation('');
    setErrorMessage('');
  }

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

      <div className="lyric-card__lyrics" role="region" aria-label="Song lyrics" ref={lyricsRef}>
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
      {isOpen &&
        createPortal(
          <aside
            ref={popoverRef}
            className="translation-popover"
            aria-label="Translation"
            aria-live="polite"
            style={{ left: `${popoverPosition.left}px`, top: `${popoverPosition.top}px` }}
          >
            {translation ? (
              <>
                <p className="translation-popover__label">Machine translation · English</p>
                <p className="translation-popover__text">{translation}</p>
              </>
            ) : errorMessage ? (
              <p className="translation-popover__message" role="alert">{errorMessage}</p>
            ) : selectedText.length > MAX_TRANSLATION_LENGTH ? (
              <p className="translation-popover__message">
                Select no more than {MAX_TRANSLATION_LENGTH.toLocaleString()} characters.
              </p>
            ) : isTranslating ? (
              <p className="translation-popover__message" role="status">Translating…</p>
            ) : (
              <button
                type="button"
                className="translation-popover__translate"
                onMouseDown={(event) => event.preventDefault()}
                onClick={() => void handleTranslate()}
              >
                Translate selection
              </button>
            )}
            <button
              type="button"
              className="translation-popover__close"
              aria-label="Close translation"
              onMouseDown={(event) => event.preventDefault()}
              onClick={closeTranslation}
            >
              Close
            </button>
          </aside>,
          document.body,
        )}
    </article>
  );
}
