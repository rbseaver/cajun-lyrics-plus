import './index.css';
import { SearchForm } from './components/SearchForm';
import { LyricCard } from './components/LyricCard';
import { EmptyState } from './components/EmptyState';
import { ErrorMessage } from './components/ErrorMessage';
import { Changelog } from './components/Changelog';
import { useLyricSearch } from './hooks/useLyricSearch';

function App() {
  const appVersion = import.meta.env.VITE_APP_VERSION ?? 'development';
  const { status, result, results, errorMessage, searchResults, selectResult } = useLyricSearch();

  return (
    <div className="app">
      <header className="app-header">
        <div className="app-header__logo">
          <svg
            className="app-header__logo-icon"
            aria-hidden="true"
            xmlns="http://www.w3.org/2000/svg"
            width="36"
            height="36"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <path d="M9 18V5l12-2v13" />
            <circle cx="6" cy="18" r="3" />
            <circle cx="18" cy="16" r="3" />
          </svg>
          <h1 className="app-header__title">FrancoLyrics</h1>
        </div>
        <p className="app-header__subtitle">
          Lyrics from French North America
        </p>
      </header>

      <main className="app-main" id="main-content">
        <SearchForm onSearch={searchResults} isLoading={status === 'loading'} />

        {status === 'idle' && <EmptyState />}

        {status === 'success' && result && <LyricCard result={result} />}

        {status === 'results' && results.length > 0 && (
          <section className="search-results" aria-label="Search results">
            <h2 className="search-results__title">Matching lyrics</h2>
            <ul className="search-results__list">
              {results.map((item) => (
                <li key={item.id ?? `${item.artist}-${item.title}`}>
                  <button
                    type="button"
                    className="search-results__button"
                    onClick={() => void selectResult(item)}
                  >
                    <span className="search-results__artist">{item.artist}</span>
                    <span className="search-results__title">{item.title}</span>
                  </button>
                </li>
              ))}
            </ul>
          </section>
        )}

        {status === 'not_found' && (
          <ErrorMessage
            message={errorMessage ?? 'No lyrics found for that search.'}
            isNotFound
          />
        )}

        {status === 'error' && (
          <ErrorMessage
            message={errorMessage ?? 'Something went wrong. Please try again.'}
          />
        )}
      </main>

      <footer className="app-footer">
        <p>
          Powered by{' '}
          <a href="https://www.cajunlyrics.com" target="_blank" rel="noopener noreferrer">
            CajunLyrics.com
          </a>
        </p>
        <Changelog version={appVersion} />
      </footer>
    </div>
  );
}

export default App;
