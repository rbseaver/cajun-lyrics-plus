import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { SearchForm } from '../SearchForm';

describe('SearchForm', () => {
  it('disables the submit button until the user enters an artist or title', () => {
    render(<SearchForm onSearch={vi.fn()} isLoading={false} />);

    expect(screen.getByRole('button', { name: /search for lyrics/i })).toBeDisabled();
  });

  it('submits a normalized search payload for artist, title, and language', async () => {
    const user = userEvent.setup();
    const onSearch = vi.fn();

    render(<SearchForm onSearch={onSearch} isLoading={false} />);

    await user.type(screen.getByLabelText(/artist/i), '  Zachary Richard  ');
    await user.type(screen.getByLabelText(/song title/i), ' Travailler C\'est Trop Dur ');
    await user.selectOptions(screen.getByLabelText(/language/i), 'fr');
    await user.click(screen.getByRole('button', { name: /search for lyrics/i }));

    expect(onSearch).toHaveBeenCalledWith({
      artist: 'Zachary Richard',
      title: "Travailler C'est Trop Dur",
      language: 'fr',
    });
  });

  it('does not submit when the form is loading', async () => {
    const user = userEvent.setup();
    const onSearch = vi.fn();

    render(<SearchForm onSearch={onSearch} isLoading />);

    await user.type(screen.getByLabelText(/artist/i), 'Zachary Richard');
    await user.click(screen.getByRole('button', { name: /search for lyrics/i }));

    expect(onSearch).not.toHaveBeenCalled();
  });
});
