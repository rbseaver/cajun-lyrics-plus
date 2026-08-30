---
trigger: always_on
---

## Frontend Project (CajunLyrics.Web)

The frontend is a Vite + React 19 + TypeScript SPA located in `CajunLyrics.Web/`.

### Stack
- **Bundler**: Vite 8 (`react-ts` template)
- **UI**: React 19 with functional components and hooks only — no class components
- **Language**: TypeScript with `erasableSyntaxOnly: true` — avoid parameter property shorthand (`public readonly x`) and `enum`; use explicit field declarations and `const` object maps instead
- **Linting**: oxlint via `.oxlintrc.json` (react + typescript + oxc plugins)
- **Styling**: Vanilla CSS in `src/index.css` — no CSS-in-JS, no Tailwind

### Testing
- **Runner**: Vitest (Vite-native, `globals: true`)
- **DOM**: jsdom environment
- **Components**: React Testing Library (`@testing-library/react`) + `@testing-library/user-event`
- **Matchers**: `@testing-library/jest-dom` (imported in `src/test/setup.ts`)
- **TDD is mandatory**: write a failing test first, confirm it is RED, then write only enough production code to make it GREEN, then refactor
- **Test location**: co-located under `src/**/__tests__/` with `.test.ts` / `.test.tsx` extensions
- **Run tests**: `npm test` (watch) or `npm run test:run` (single pass)

### API Proxy
Vite dev server proxies `/api/*` → `https://localhost:7156` with `secure: false` (self-signed cert).
The .NET API runs on `https://localhost:7156`.

### Conventional Commits — Frontend Scopes
| Type | Scope | Example |
|------|-------|---------|
| New feature | `feat(web)` | `feat(web): add lyric card component` |
| Bug fix | `fix(web)` | `fix(web): trim whitespace from search inputs` |
| Tests | `test(web)` | `test(web): add useLyricSearch hook tests` |
| Styling/CSS | `style(web)` | `style(web): update colour palette tokens` |
| Config/tooling | `chore(web)` | `chore(web): add Vitest configuration` |
