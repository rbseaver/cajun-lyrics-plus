# LLM Handoff Log

This is a running project-context log for LLM sessions, not a replacement for the public `CHANGELOG.md`. Entries are newest first and capture the intent, important implementation decisions, validation, and any useful next steps.

When finishing meaningful repository work, add a concise, factual entry at the top. Include relevant files, decisions, and validation results; preserve older entries. Do not include secrets or sensitive data.

## 2026-11-18 — Render the changelog on its own page

- Replaced the footer's expandable changelog with a `/changelog` page that renders the full markdown document in normal page flow. The FrancoLyrics header link and a prominent “Back to lyrics” link return to `/`.
- Added route and navigation regression tests. Validation passed: 29 frontend tests, lint, production build, and `git diff --check`.

## 2026-11-18 — Position translation popover by selected text

- Replaced the fixed bottom-right translation popover placement with selection-anchored positioning. The popover flips above the selection or clamps to the viewport when space is limited.
- Added a LyricCard regression assertion for positioning relative to the selected lyric range. Validation passed: 28 frontend tests, lint, production build, and `git diff --check`.

## 2026-11-17 — Pass DeepL key into Docker Compose

- Updated `docker-compose.yml` to pass the host's `DEEPL_API_KEY` into the API container without adding the key to the image or repository.
- Validation passed: `docker compose config --quiet` and `git diff --check`.

## 2026-11-17 — Translate selected lyric text with DeepL

- Added an explicit selection-to-translate popover for lyric text, defaulting to English and marking results as machine translations. Selection is capped at 5,000 characters in both the UI and API.
- Added `POST /api/Translation/translate` and a DeepL Free API client; the API reads `DEEPL_API_KEY` from its process environment, so the key is never sent to the browser. The app starts without a key and returns 503 for translation until configured.
- Validation passed: 77 backend unit tests, 22 non-live integration tests, 28 frontend tests, frontend lint and production build, and `git diff --check`. No live DeepL request was made. Integration test restore still reports the existing moderate NU1902 advisory for `Microsoft.AspNetCore.WebUtilities` 2.3.0.

## 2026-10-25 — Use Render's deployed commit SHA

- The Dockerfile now falls back to Render's build-provided `RENDER_GIT_COMMIT` when `APP_COMMIT` is unset, preserving the existing GitLab CI argument. GitHub Actions now builds with the Render argument name to cover that path in CI.
- Validation passed: Docker image built with a simulated `RENDER_GIT_COMMIT`, and the full SHA was found in the bundled frontend; `git diff --check` passed.

## 2026-10-25 — Render changelog in the frontend

- Replaced the footer's GitHub changelog link with an accessible disclosure that renders the bundled `CHANGELOG.md` via `react-markdown`; raw HTML is not enabled. Added Vite filesystem access to the repository root and copied the changelog into the Docker frontend build stage.
- Added a regression test for opening the release notes. Validation passed: 25 frontend tests, lint, production build, Docker image build, and `git diff --check`.

## 2026-10-25 — Link footer version to changelog

- Made the footer's version label link to the repository's `main/CHANGELOG.md` on GitHub, which works from the deployed app.
- Consolidated the legacy `0.0.0` feature and bug-fix entries under the `1.0.0` release and removed the standalone `0.0.0` section.
- Validation passed: 25 frontend tests, lint, production build, and `git diff --check`.

## 2026-10-25 — Show release version in the footer

- Added a footer version label sourced from Vite's `VITE_APP_VERSION`, with `development` for non-image local frontend builds. The Dockerfile composes the root package version and full CI commit SHA as SemVer build metadata (`1.5.4+sha.<full-sha>`).
- Both GitHub Actions and GitLab image builds now pass their commit SHA into Docker. The root package version is read at image build time, so release-it version bumps flow into the deployed UI without another release-specific edit.
- Added footer regression coverage. Validation passed: all 25 frontend tests, lint, production frontend build, Docker image build, and `git diff --check`.

## 2026-10-25 — Avoid duplicate Actions run numbers

- Updated `.github/workflows/ci.yml` so tag-triggered custom titles contain the tag and full commit SHA, while other runs use `CI`. GitHub appends the unique workflow run number to the displayed title, so it is intentionally omitted from `run-name` to avoid duplication.
- No job behavior or GitLab CI configuration changed.

## 2026-10-25 — Validate API search inputs

- Added controller validation for blank search criteria, blank artist/title in direct lyric lookup, nonpositive lyric IDs, unsupported language filters, and language lists longer than the supported set. Library-level custom language support remains unchanged.
- Added unit and HTTP integration coverage. Validation passed: 68 unit tests, 20 non-live integration tests, and `git diff --check`.

## 2026-10-25 — Add GitHub Actions CI

- Added `.github/workflows/ci.yml` alongside the unchanged `.gitlab-ci.yml`. Actions runs on pushes to `main`, tags, and pull requests targeting `main`, with frontend, unit, hermetic integration, Docker build, and tag-only release gates.
- Kept the live-upstream integration canary excluded and uploaded test/coverage results as seven-day artifacts. Updated `.github/copilot-instructions.md` to document both CI systems.
- Validation passed: workflow YAML and gate assertions, 24 frontend tests, 58 .NET unit tests, 17 non-live integration tests, and the Docker image build.

## 2026-10-25 — Create a durable LLM handoff log

- Added this tracked log and linked it from `AGENTS.md` and `.github/copilot-instructions.md`.
- No application code changed; documentation-only change.

## 2026-10-25 — Fetch selected lyrics by upstream ID

- Fixed lyric retrieval for search results whose exact title could not be resolved reliably by the upstream artist/title search. For example, “Ça, Ça C'est Dur” was found as ID `1054`, while title lookup could return a different song (ID `1065`).
- The frontend now fetches a selected result by its upstream ID; the API and library support ID-based retrieval while preserving artist/title lookup for compatibility. The service verifies the upstream response ID to prevent returning a different song.
- Added regression coverage and a local WireMock fixture. Relevant areas: `CajunLyrics.Api/Controllers/LyricsController.cs`, `CajunLyrics.Lib/Services/`, `CajunLyrics.Lib/Helpers/RequestUtilities.cs`, `CajunLyrics.Web/src/`, and the unit/integration test projects.
- Validation passed: frontend tests (24), lint, and production build; .NET solution build (0 warnings/errors), 58 unit tests, and 17 non-live integration tests. Live-upstream tests were excluded.
- Released and pushed as v1.5.3. `main` is at `67e8042` (`chore: release v1.5.3`); fix commit is `953ff61`.
- No known follow-up work remains. Search-result IDs are the reliable identity for selecting a lyric; the legacy artist/title path remains supported for existing clients.

## 2026-10-24 — Apply the Acadian Maritime palette

- Updated `CajunLyrics.Web/src/index.css` to use deep navy surfaces, Atlantic blue for interaction, and aged gold for cultural accents and metadata. Kept the existing layout and long-lyric readability; reduced the search button's visual dominance.
- Frontend tests (24) and production build passed.
- Committed as `6c1a3b8 style(web): apply Acadian Maritime color palette`.
