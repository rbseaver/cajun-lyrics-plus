# Frontend Testing Guide

This project uses Vite + React 19 + TypeScript with Vitest and React Testing Library for frontend unit tests.

## Tech stack

- Vite 8
- React 19
- TypeScript
- Vitest
- jsdom
- React Testing Library
- @testing-library/user-event
- @testing-library/jest-dom

## Configuration

Vitest is configured in `vite.config.ts` with:

- `globals: true`
- `environment: 'jsdom'`
- `setupFiles: './src/test/setup.ts'`
- coverage collection enabled for `src/**/*.{ts,tsx}`

## Test conventions

- Place tests under `src/**/__tests__/` or `src/__tests__/`
- Use `.test.ts` or `.test.tsx` file extensions
- Prefer user-driven behavior tests over implementation detail tests
- Follow TDD: write a failing test first, then code until it passes, then refactor

## Commands

From `CajunLyrics.Web/`:

- `npm test` — watch mode
- `npm run test:run` — single pass
- `npm run build` — production build

## Current test coverage

The frontend suite currently covers:

- search form validation and submission
- lyric search hook state transitions
- top-level app rendering for idle, success, and not-found states
