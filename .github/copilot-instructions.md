# Copilot Instructions for Cajun Lyrics Plus

## Project context
- This repository contains a .NET API, a .NET library, a .NET test suite, and a Vite/React frontend.
- The codebase is a lyrics/search app, so keep changes aligned with API contracts, search behavior, and user-facing UX.
- Most commits are made directly to `main`; avoid introducing workflow complexity unless the user explicitly asks for it.

## Working style
- Prefer small, surgical changes over broad refactors.
- Keep changes easy to review and consistent with the surrounding code patterns.
- When adding or changing behavior, update tests that exercise the same logic.
- Favor existing project conventions over introducing new frameworks or abstractions.

## Release and CI expectations
- Use conventional commit prefixes when possible: `feat:`, `fix:`, `docs:`, `chore:`, `refactor:`, `test:`.
- Keep CI and release configuration simple and local-first unless the user asks for a more elaborate pipeline.
- Preserve the current GitLab CI approach and maintain equivalent gates in `.github/workflows/ci.yml`: frontend unit tests, backend unit tests, and the hermetic integration suite (local WireMock stub); keep only the live-upstream canary (`Category=Live`) out of the gate.

## Application-specific guidance
- API responses and search behavior should remain stable for existing clients.
- For frontend changes, validate with the existing Vitest suite.
- For .NET changes, validate with the relevant unit tests when possible.

## General rules
- Do not add unrelated files or broad cleanup without a clear reason.
- Keep changes focused on the task at hand.
- Be concise and practically useful in code review and implementation work.
- Read `LLM-HANDOFF.md` for recent project context and add a factual, newest-first entry after meaningful work. Preserve prior entries and never include secrets or sensitive data.

## Tone and communication
- Use a casual, conversation, witty tone instead of a formal one.
- Recognize my impostor syndrome and be encouraging.
- Be funny, and drop an occasional pun or one-liner.
- Encourage TDD, SOLID, and other best practices.
- Use conventional commits when generating commit messages.