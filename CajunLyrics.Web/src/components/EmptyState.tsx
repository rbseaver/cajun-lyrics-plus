export function EmptyState() {
  return (
    <div className="empty-state" aria-label="Welcome message">
      <div className="empty-state__icon" aria-hidden="true">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width="64"
          height="64"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <path d="M9 18V5l12-2v13" />
          <circle cx="6" cy="18" r="3" />
          <circle cx="18" cy="16" r="3" />
        </svg>
      </div>
      <h2 className="empty-state__heading">Find Your Cajun Lyrics</h2>
      <p className="empty-state__body">
        Enter an artist name and/or song title above to search thousands of Cajun, Creole,
        and Louisiana French songs.
      </p>
      <ul className="empty-state__suggestions" aria-label="Search suggestions">
        <li>
          <span className="suggestion-chip">Zachary Richard</span>
        </li>
        <li>
          <span className="suggestion-chip">Beausoleil</span>
        </li>
        <li>
          <span className="suggestion-chip">Steve Riley</span>
        </li>
      </ul>
    </div>
  );
}
