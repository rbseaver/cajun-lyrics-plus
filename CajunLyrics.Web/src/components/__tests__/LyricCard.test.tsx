import { render, screen } from '@testing-library/react';
import { LyricCard } from '../LyricCard';
import type { LyricResult } from '../../types/lyrics';

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
});
