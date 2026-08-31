import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import App from '../App';

describe('App integration', () => {
  it('runs the full lyric search flow with a mocked fetch response', async () => {
    const user = userEvent.setup();
    const mockResponse = {
      ok: true,
      status: 200,
      json: async () => ({
        id: 12,
        artist: 'Zachary Richard',
        title: "Travailler C'est Trop Dur",
        lyric: 'Là on travaille\nEt ça va mieux',
      }),
    };

    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(mockResponse));

    render(<App />);

    await user.type(screen.getByLabelText(/artist/i), 'Zachary Richard');
    await user.type(screen.getByLabelText(/song title/i), "Travailler C'est Trop Dur");
    await user.click(screen.getByRole('button', { name: /search for lyrics/i }));

    await waitFor(() => {
      expect(screen.getByLabelText(/lyrics for travailler c'est trop dur/i)).toBeInTheDocument();
    });

    expect(fetch).toHaveBeenCalledWith(
      "/api/Lyrics/lyric?artist=Zachary+Richard&title=Travailler+C%27est+Trop+Dur",
      { headers: { Accept: 'application/json' } },
    );
    expect(screen.getByText(/là on travaille/i)).toBeInTheDocument();
  });
});
