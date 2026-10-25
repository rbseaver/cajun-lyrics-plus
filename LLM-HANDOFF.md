# LLM Handoff Log

This is a running project-context log for LLM sessions, not a replacement for the public `CHANGELOG.md`. Entries are newest first and capture the intent, important implementation decisions, validation, and any useful next steps.

When finishing meaningful repository work, add a concise, factual entry at the top. Include relevant files, decisions, and validation results; preserve older entries. Do not include secrets or sensitive data.

## 2026-10-25 — Include release identity in Actions run names

- Updated `.github/workflows/ci.yml` so tag-triggered runs are named with the tag, full commit SHA, and workflow run number (for example, `v1.5.4-<full-sha>.42`). Other runs retain the `CI #<run number>` name.
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
