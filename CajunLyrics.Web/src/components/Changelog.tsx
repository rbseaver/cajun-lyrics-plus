import ReactMarkdown from 'react-markdown';
import changelog from '../../../CHANGELOG.md?raw';

interface ChangelogProps {
  version: string;
}

export function Changelog({ version }: ChangelogProps) {
  return (
    <details className="app-footer__changelog">
      <summary className="app-footer__version">Version {version}</summary>
      <div className="app-footer__changelog-content">
        <ReactMarkdown>{changelog}</ReactMarkdown>
      </div>
    </details>
  );
}
