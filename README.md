# Cajun Lyrics Plus

[![pipeline status](https://gitlab.com/rbseaver/cajun-lyrics-plus/badges/main/pipeline.svg)](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/pipelines)
[![coverage](https://gitlab.com/rbseaver/cajun-lyrics-plus/badges/main/coverage.svg)](https://gitlab.com/rbseaver/cajun-lyrics-plus/-/jobs)

## Release and CI process

This repository uses a simple local release flow plus GitLab CI for automated validation.

### Local release flow

This project uses `standard-version` to generate a changelog and create a semantic-version tag locally.

```bash
npm install
npm run release -- --first-release
```

This will:

- analyze conventional commits
- update `CHANGELOG.md`
- bump the version and write it back to `package.json`
- create a git tag like `v1.0.0`

### GitLab CI

The GitLab pipeline runs on both pushes to `main` and on version tags.

- it runs frontend Vitest unit tests in `CajunLyrics.Web` using Node 22+
- it collects frontend coverage with V8
- it restores the .NET solution and runs the existing NUnit unit test suite
- it collects backend coverage with `XPlat Code Coverage`
- it blocks release tags if either test suite fails

The integration test suite is intentionally not part of the CI gate right now because it is more network-dependent and can be flaky.

### Commit conventions

Use conventional commits so releases are accurate:

- `feat:` for features
- `fix:` for bug fixes
- `feat!:` or `fix!:` for breaking changes
- `docs:`, `chore:`, `refactor:`, `test:` for non-user-facing work
