import ReactMarkdown from 'react-markdown';
import changelog from '../../../CHANGELOG.md?raw';

export function Changelog() {
  return (
    <article className="changelog-page__content">
      <ReactMarkdown>{changelog}</ReactMarkdown>
    </article>
  );
}
