# Cajun Lyrics Plus

## Local release process

This repository uses `standard-version` to generate a changelog and create a semantic-version tag locally.

### Commands

```bash
npm install
npm run release -- --first-release
```

This will:

- analyze conventional commits
- update `CHANGELOG.md`
- bump the version and write it back to `package.json`
- create a git tag like `v1.0.0`

### Commit conventions

Use conventional commits so releases are accurate:

- `feat:` for features
- `fix:` for bug fixes
- `feat!:` or `fix!:` for breaking changes
- `docs:`, `chore:`, `refactor:`, `test:` for non-user-facing work
