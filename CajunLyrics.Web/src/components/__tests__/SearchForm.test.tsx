import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { SearchForm } from '../SearchForm';
import { ANY_LANGUAGE } from '../../types/lyrics';

describe('SearchForm', () => {
  it('disables the submit button until the user enters an artist or title', () => {
    render(<SearchForm onSearch={vi.fn()} isLoading={false} />);

    expect(screen.getByRole('button', { name: /search for lyrics/i })).toBeDisabled();
  });

  it('hides the language select behind an advanced search toggle', async () => {
    const user = userEvent.setup();

    render(<SearchForm onSearch={vi.fn()} isLoading={false} />);

    expect(screen.queryByLabelText(/^language$/i)).not.toBeInTheDocument();

    const toggle = screen.getByRole('button', { name: /advanced search/i });
    expect(toggle).toHaveAttribute('aria-expanded', 'false');

    await user.click(toggle);

    expect(screen.getByLabelText(/^language$/i)).toBeInTheDocument();
    expect(toggle).toHaveAttribute('aria-expanded', 'true');
  });

  it('offers any language plus each concrete language in advanced search', async () => {
    const user = userEvent.setup();

    render(<SearchForm onSearch={vi.fn()} isLoading={false} />);

    await user.click(screen.getByRole('button', { name: /advanced search/i }));

    const select = screen.getByLabelText(/^language$/i) as HTMLSelectElement;
    const values = Array.from(select.options).map((option) => option.value);

    expect(values).toEqual(['', ANY_LANGUAGE, 'fr', 'fr_LA', 'fr_QC', 'fr_AC', 'en']);
  });

  it('submits without a language so the API applies its regional French default', async () => {
    const user = userEvent.setup();
    const onSearch = vi.fn();

    render(<SearchForm onSearch={onSearch} isLoading={false} />);

    await user.type(screen.getByLabelText(/artist/i), '  Zachary Richard  ');
    await user.type(screen.getByLabelText(/song title/i), " Travailler C'est Trop Dur ");
    await user.click(screen.getByRole('button', { name: /search for lyrics/i }));

    expect(onSearch).toHaveBeenCalledWith({
      artist: 'Zachary Richard',
      title: "Travailler C'est Trop Dur",
      languages: undefined,
    });
  });

  it('submits the language chosen in advanced search', async () => {
    const user = userEvent.setup();
    const onSearch = vi.fn();

    render(<SearchForm onSearch={onSearch} isLoading={false} />);

    await user.type(screen.getByLabelText(/artist/i), 'Zachary Richard');
    await user.click(screen.getByRole('button', { name: /advanced search/i }));
    await user.selectOptions(screen.getByLabelText(/^language$/i), 'en');
    await user.click(screen.getByRole('button', { name: /search for lyrics/i }));

    expect(onSearch).toHaveBeenCalledWith({
      artist: 'Zachary Richard',
      title: undefined,
      languages: ['en'],
    });
  });

  it('submits the any-language option as the ALL sentinel', async () => {
    const user = userEvent.setup();
    const onSearch = vi.fn();

    render(<SearchForm onSearch={onSearch} isLoading={false} />);

    await user.type(screen.getByLabelText(/artist/i), 'Zachary Richard');
    await user.click(screen.getByRole('button', { name: /advanced search/i }));
    await user.selectOptions(screen.getByLabelText(/^language$/i), ANY_LANGUAGE);
    await user.click(screen.getByRole('button', { name: /search for lyrics/i }));

    expect(onSearch).toHaveBeenCalledWith({
      artist: 'Zachary Richard',
      title: undefined,
      languages: [ANY_LANGUAGE],
    });
  });

  it('clears the language when advanced search is collapsed', async () => {
    const user = userEvent.setup();
    const onSearch = vi.fn();

    render(<SearchForm onSearch={onSearch} isLoading={false} />);

    await user.type(screen.getByLabelText(/artist/i), 'Zachary Richard');
    await user.click(screen.getByRole('button', { name: /advanced search/i }));
    await user.selectOptions(screen.getByLabelText(/^language$/i), 'en');
    await user.click(screen.getByRole('button', { name: /advanced search/i }));
    await user.click(screen.getByRole('button', { name: /search for lyrics/i }));

    expect(onSearch).toHaveBeenCalledWith({
      artist: 'Zachary Richard',
      title: undefined,
      languages: undefined,
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
