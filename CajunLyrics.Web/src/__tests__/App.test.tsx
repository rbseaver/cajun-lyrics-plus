import { render, screen } from '@testing-library/react';
import App from '../App';
import { useLyricSearch } from '../hooks/useLyricSearch';

vi.mock('../hooks/useLyricSearch', () => ({
  useLyricSearch: vi.fn(),
}));

describe('App', () => {
  it('renders the empty state when the app is idle', () => {
    vi.mocked(useLyricSearch).mockReturnValue({
      status: 'idle',
      result: null,
      results: [],
      errorMessage: null,
      search: vi.fn(),
      searchResults: vi.fn(),
      selectResult: vi.fn(),
      reset: vi.fn(),
    });

    render(<App />);

    expect(screen.getByLabelText(/welcome message/i)).toBeInTheDocument();
    expect(screen.getByText(/find your cajun lyrics/i)).toBeInTheDocument();
  });

  it('renders lyric results when the search succeeds', () => {
    vi.mocked(useLyricSearch).mockReturnValue({
      status: 'success',
      result: {
        id: 5,
        artist: 'Zachary Richard',
        title: 'Travailler C\'est Trop Dur',
        lyric: 'Là on travaille\nEt ça va mieux',
      },
      results: [],
      errorMessage: null,
      search: vi.fn(),
      searchResults: vi.fn(),
      selectResult: vi.fn(),
      reset: vi.fn(),
    });

    render(<App />);

    expect(screen.getByLabelText(/lyrics for travailler c'est trop dur/i)).toBeInTheDocument();
    expect(screen.getByText(/là on travaille/i)).toBeInTheDocument();
  });

  it('renders a list of matching results when the search returns more than one match', () => {
    vi.mocked(useLyricSearch).mockReturnValue({
      status: 'results',
      result: null,
      results: [
        { id: 11, artist: 'Zachary Richard', title: 'Travailler C\'est Trop Dur' },
        { id: 12, artist: 'Zachary Richard', title: 'Un Peu De Temps' },
      ],
      errorMessage: null,
      search: vi.fn(),
      searchResults: vi.fn(),
      selectResult: vi.fn(),
      reset: vi.fn(),
    });

    render(<App />);

    expect(screen.getByLabelText(/search results/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /travailler c'est trop dur/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /un peu de temps/i })).toBeInTheDocument();
  });

  it('renders a not found message when the search misses', () => {
    vi.mocked(useLyricSearch).mockReturnValue({
      status: 'not_found',
      result: null,
      results: [],
      errorMessage: 'No lyrics found for that artist and title.',
      search: vi.fn(),
      searchResults: vi.fn(),
      selectResult: vi.fn(),
      reset: vi.fn(),
    });

    render(<App />);

    expect(screen.getByRole('alert')).toHaveTextContent('No lyrics found for that artist and title.');
  });
});
