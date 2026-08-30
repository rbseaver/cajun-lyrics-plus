import './index.css';
import { SearchForm } from './components/SearchForm';
import { LyricCard } from './components/LyricCard';
import { EmptyState } from './components/EmptyState';
import { ErrorMessage } from './components/ErrorMessage';
import { useLyricSearch } from './hooks/useLyricSearch';

function App() {
  const { status, result, errorMessage, search } = useLyricSearch();

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
          <h1 className="app-header__title">CajunLyrics+</h1>
        </div>
        <p className="app-header__subtitle">
          Cajun, Creole &amp; Louisiana French song lyrics
        </p>
      </header>

      <main className="app-main" id="main-content">
        <SearchForm onSearch={search} isLoading={status === 'loading'} />

        {status === 'idle' && <EmptyState />}

        {status === 'success' && result && <LyricCard result={result} />}

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
      </footer>
    </div>
  );
}

export default App;
