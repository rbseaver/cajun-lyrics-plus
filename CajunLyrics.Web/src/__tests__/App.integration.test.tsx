import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import App from '../App';

describe('App integration', () => {
  it('runs the full lyric search flow with a mocked fetch response', async () => {
    const user = userEvent.setup();

    const searchResultsResponse = {
      ok: true,
      status: 200,
      json: async () => ({
        lyricResults: [
          {
            id: 12,
            artist: 'Zachary Richard',
            title: "Travailler C'est Trop Dur",
          },
        ],
      }),
    };

    const lyricResponse = {
      ok: true,
      status: 200,
      json: async () => ({
        id: 12,
        artist: 'Zachary Richard',
        title: "Travailler C'est Trop Dur",
        lyric: 'Là on travaille\nEt ça va mieux',
        language: { code: 'fr_LA', displayName: 'Louisiana French' },
      }),
    };

    vi.stubGlobal('fetch', vi.fn().mockResolvedValueOnce(searchResultsResponse).mockResolvedValueOnce(lyricResponse));

    render(<App />);

    await user.type(screen.getByLabelText(/artist/i), 'Zachary Richard');
    await user.type(screen.getByLabelText(/song title/i), "Travailler C'est Trop Dur");
    await user.click(screen.getByRole('button', { name: /search for lyrics/i }));

    await waitFor(() => {
      expect(screen.getByLabelText(/lyrics for travailler c'est trop dur/i)).toBeInTheDocument();
    });

    expect(fetch).toHaveBeenNthCalledWith(
      1,
      "/api/Lyrics/search?artist=Zachary+Richard&title=Travailler+C%27est+Trop+Dur",
      { headers: { Accept: 'application/json' } },
    );
    expect(fetch).toHaveBeenNthCalledWith(
      2,
      '/api/Lyrics/lyric?id=12',
      { headers: { Accept: 'application/json' } },
    );
    expect(screen.getByText(/là on travaille/i)).toBeInTheDocument();
  });

  it('runs an advanced English search and fetches the selected song by ID', async () => {
    const user = userEvent.setup();

    const searchResultsResponse = {
      ok: true,
      status: 200,
      json: async () => ({
        lyricResults: [
          { id: 1, artist: 'Zachary Richard', title: 'American Tune' },
          { id: 2, artist: 'Zachary Richard', title: 'Other English Song' },
        ],
      }),
    };

    const lyricResponse = {
      ok: true,
      status: 200,
      json: async () => ({
        id: 1,
        artist: 'Zachary Richard',
        title: 'American Tune',
        lyric: 'Let us be lovers',
      }),
    };

    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValueOnce(searchResultsResponse).mockResolvedValueOnce(lyricResponse),
    );

    render(<App />);

    await user.type(screen.getByLabelText(/artist/i), 'Zachary Richard');
    await user.click(screen.getByRole('button', { name: /advanced search/i }));
    await user.selectOptions(screen.getByLabelText(/^language$/i), 'en');
    await user.click(screen.getByRole('button', { name: /search for lyrics/i }));

    await waitFor(() => {
      expect(screen.getByText('American Tune')).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: /american tune/i }));

    await waitFor(() => {
      expect(screen.getByText(/let us be lovers/i)).toBeInTheDocument();
    });

    expect(fetch).toHaveBeenNthCalledWith(
      1,
      '/api/Lyrics/search?artist=Zachary+Richard&language=en',
      { headers: { Accept: 'application/json' } },
    );
    expect(fetch).toHaveBeenNthCalledWith(
      2,
      '/api/Lyrics/lyric?id=1&language=en',
      { headers: { Accept: 'application/json' } },
    );
  });
});
