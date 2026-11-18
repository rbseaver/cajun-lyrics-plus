import { fireEvent, render, screen } from '@testing-library/react';
import { LyricCard } from '../LyricCard';
import type { LyricResult } from '../../types/lyrics';
import userEvent from '@testing-library/user-event';

describe('LyricCard', () => {
  const baseResult: LyricResult = {
    id: 512,
    artist: 'Feufollet',
    title: 'Blues De Dix Ans',
    lyricsUrl: 'https://www.cajunlyrics.com/lyric/blues-de-dix-ans',
    artistUrl: 'https://www.cajunlyrics.com/artist/feufollet',
    lyric: 'Dix ans, dix ans',
  };

  it('shows the matched language below the artist', () => {
    render(<LyricCard result={{ ...baseResult, language: { code: 'fr_LA', displayName: 'Louisiana French' } }} />);

    expect(screen.getByText('Louisiana French')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /view feufollet on cajunlyrics/i })).toBeInTheDocument();
  });

  it('shows the language below the plain artist name when there is no artist link', () => {
    render(
      <LyricCard
        result={{
          ...baseResult,
          artistUrl: undefined,
          language: { code: 'en', displayName: 'English' },
        }}
      />,
    );

    expect(screen.getByText('English')).toBeInTheDocument();
    expect(screen.getByText('Feufollet')).toBeInTheDocument();
  });

  it('hides the language when it is unknown', () => {
    render(<LyricCard result={{ ...baseResult, language: undefined }} />);

    expect(screen.queryByText(/french|english/i)).not.toBeInTheDocument();
  });

  it('translates selected lyric text into English when requested', async () => {
    const user = userEvent.setup();
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({ translation: 'Ten years, ten years' }),
      }),
    );
    render(<LyricCard result={baseResult} />);

    const lyricLine = screen.getByText('Dix ans, dix ans');
    const range = document.createRange();
    range.selectNodeContents(lyricLine);
    Object.assign(range, { getBoundingClientRect: () => new DOMRect(120, 220, 80, 20) });
    const selection = window.getSelection();
    selection?.removeAllRanges();
    selection?.addRange(range);
    fireEvent(document, new Event('selectionchange'));

    expect(await screen.findByRole('complementary', { name: 'Translation' })).toHaveStyle({
      left: '120px',
      top: '248px',
    });

    await user.click(await screen.findByRole('button', { name: /translate selection/i }));

    expect(await screen.findByText('Ten years, ten years')).toBeInTheDocument();
    expect(screen.getByText(/machine translation/i)).toBeInTheDocument();
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    window.getSelection()?.removeAllRanges();
  });
});
