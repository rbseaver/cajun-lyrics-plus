# Cajun Lyrics Plus

[![pipeline status](https://gitlab.com/rbseaver/cajun-lyrics-plus/badges/main/pipeline.svg)](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/pipelines)
[![coverage](https://gitlab.com/rbseaver/cajun-lyrics-plus/badges/main/coverage.svg)](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/jobs)

## Local Docker monolith

This repo is set up as a single Dockerized app: the backend serves the built frontend as static files, so you can run the whole project locally from one container.

```bash
docker compose up --build
```

Then open:

- http://localhost:8080

This runs the ASP.NET API and the React frontend together in one deployment unit, which keeps the local dev experience simple while still using the same backend/frontend structure as the app itself.

## Release and CI process

This repository uses a simple local release flow plus GitLab CI for automated validation.

### Local release flow

This project uses `release-it` with its Conventional Commits plugin to generate a changelog, bump the version, and create a semantic-version tag locally.

```bash
npm install
npm run release
```

This will:

- analyze conventional commits
- update `CHANGELOG.md`
- bump the version and write it back to `package.json`
- create a git tag like `v1.0.0`

The release command creates the release commit and tag locally but does not push them. Review the result, then push when ready:

```bash
git push origin main --follow-tags
```

Use `npm run release -- --dry-run` to preview a release without changing files. Releases require a clean working tree.

### GitLab CI

The GitLab pipeline runs on both pushes to `main` and on version tags.

- it runs frontend Vitest unit tests in `CajunLyrics.Web` using Node 22+
- it collects frontend coverage with V8
- it restores the .NET solution and runs the existing NUnit unit test suite
- it collects backend coverage with `XPlat Code Coverage`
- it runs the hermetic integration suite against a local WireMock stub (no network needed)
- it builds the Docker image (build only, no push) so Dockerfile breakage shows up early
- it blocks release tags if any gate fails

The hermetic integration suite runs in the gate (job `integration_tests`). Only the live-upstream canary (`Category("Live")`) is excluded — it hits the real API on purpose and is reserved for CD/deploy-time checks. If it fails, upstream likely changed and the recorded corpus in `CajunLyrics.Tests.Integration/UpstreamFixtures/` needs refreshing.

### Commit conventions

Use conventional commits so releases are accurate:

- `feat:` for features
- `fix:` for bug fixes
- `feat!:` or `fix!:` for breaking changes
- `docs:`, `chore:`, `refactor:`, `test:` for non-user-facing work
