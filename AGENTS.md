# CajunLyrics Plus

Project-level instructions for this repository. Tone, coding standards, and general working rules live in the global `~/.config/opencode/AGENTS.md` — this file only covers what is specific to this codebase.

## How instructions are layered here

Only this file is loaded automatically — OpenCode reads `AGENTS.md` and nothing else in the repo. Neither `.agents/` nor `.github/copilot-instructions.md` is auto-loaded; they stay dormant until you open them. Treat the entries below as instructions to read, not as suggestions for further reading:

- **Before any API, architecture, or design decision, read `.agents/rules/code-style-guide.md`.** One line long, so it costs nothing to read.
- **Before touching `CajunLyrics.Web`, read `.agents/rules/frontend-guide.md`.** It holds constraints you cannot guess: `erasableSyntaxOnly` (so no enums and no parameter-property shorthand — use explicit field declarations and `const` object maps), functional components only, vanilla CSS with no Tailwind or CSS-in-JS, the Vitest / React Testing Library setup, where tests live, and the frontend commit scopes (`feat(web)`, `fix(web)`, `test(web)`, ...).
- **Before building any new feature, read `.agents/workflows/red-green-refactor.md`** — the red-green-refactor cycle plus Martin's Three Laws of TDD.

Two rules for dealing with those files:

- **They are the single source of truth for their content.** Never copy their text into this file — duplicated rules drift apart. Edit them in place instead.
- **Project rules are more specific than the global file, so they win inside this repo.** The global instructions say "test-first where it makes sense"; `frontend-guide.md` and `red-green-refactor.md` require TDD outright. On this project TDD is not optional.

`.github/copilot-instructions.md` is Copilot's own instructions file — project context, working style, CI expectations, plus a copy of the tone rules. OpenCode ignores it; keep its tone section aligned with the global file if you change either.

## What this is

Two apps in one repo, deployed as a single Dockerized unit:

- **`CajunLyrics.Api`** — a C# / ASP.NET Core Web API on .NET 10.
- **`CajunLyrics.Web`** — a Vite + React 19 + TypeScript SPA.

The API wraps the external **Cajun Lyrics API** (`https://api.cajunlyrics.com`), which answers with **XML, not JSON** — responses are fetched as bytes, decoded UTF-8, and run through `XmlSerializer`.

Endpoints:

```text
GET /api/Lyrics/lyric?artist=...&title=...&language=...   # one lyric, 404 when missing
GET /api/Lyrics/search?artist=...&title=...&language=...  # multi-result search
GET /health                                               # self liveness probe for container hosts
```

`language` binds to the `LyricLanguage` smart enum (code/display name, implicit conversions to and from `string`) and maps to the upstream query parameter `lf`, not `language`.

In Development, `/` redirects to the Scalar API reference, Swagger UI reads `/openapi/v1.json`, and static files plus a `MapFallbackToFile("index.html")` catch-all serve the built SPA — that's the monolith shape.

## Layout

- **`CajunLyrics.Api`** — controllers, `Program.cs` DI wiring, OpenAPI/Scalar, static file serving.
- **`CajunLyrics.Lib`** — all domain logic: `Services` (fetch + XML deserialization), `Http`, `Configuration`, `Helpers`, `Models`.
- **`CajunLyrics.Tests.Unit`** — NUnit tests covering Api and Lib.
- **`CajunLyrics.Tests.Integration`** — endpoint tests via `WebApplicationFactory`, plus a fixture that hits the real upstream API.
- **`CajunLyrics.Web`** — the SPA: `src/components`, `src/hooks`, `src/api`, co-located tests.

Solution file is `CajunLyrics.slnx` (XML solution format, not a classic `.sln`). Package sources live in `NuGet.Config` (nuget.org only) — the old lowercase `nuget.config` is gone, don't recreate it.

**Layering rule:** controllers stay thin and delegate to `ILyricsService`. `CajunLyrics.Lib` must never reference `CajunLyrics.Api` — the dependency always points from Api to Lib.

## Commands

Run everything from inside WSL at `~/source/repos/cajun-lyrics-plus` (see Gotchas).

```bash
# Backend
dotnet build                                   # solution builds clean: 0 warnings, 0 errors
dotnet run --project CajunLyrics.Api           # https://localhost:7156, http://localhost:5118
dotnet test CajunLyrics.Tests.Unit             # fast, no network
dotnet test CajunLyrics.Tests.Integration      # includes a live-network fixture
dotnet test                                    # everything

# Frontend (in CajunLyrics.Web, node >= 22 required)
npm ci
npm run test:run                               # single pass; `npm test` is watch mode
npm run lint                                   # oxlint
npm run build                                  # tsc -b && vite build

# Whole app
docker compose up --build                      # then http://localhost:8080

# Release (root, requires a clean working tree — creates commit + tag but does not push)
npm run release
git push origin main --follow-tags
```

Dev server detail: Vite proxies `/api/*` → `https://localhost:7156` with `secure: false`, and the API allows CORS from `http://localhost:5173` via the `ReactDevServer` policy (Development only). HTTPS redirection is Development-only — in the container (`ASPNETCORE_ENVIRONMENT=Docker`) and in Production the proxy terminates TLS, so the app never redirects itself.

## CI and releases

`.gitlab-ci.yml` runs on pushes to `main` and on version tags, with two test gates plus a release gate:

- `frontend_tests` — `node:22`, `npm ci`, `npm test -- --run --coverage` in `CajunLyrics.Web`.
- `unit_tests` — `dotnet test` on the unit project with `XPlat Code Coverage`.
- `release_gate` — runs on tags only; blocks a release if either suite failed.

**The integration suite is deliberately not in the CI gate** (per README: network-dependent and flaky). Treat it as a local check.

Releases use `release-it` with the Conventional Commits plugin: it updates `CHANGELOG.md`, bumps `package.json` (root, currently `1.1.0`), and creates the `v*` tag.

## Testing conventions

- Fixtures are named for the scenario: `WhenCallingX.cs` / `WhenUsingX.cs`, with `ShouldDoSomething` methods.
- Shared plumbing lives in `CajunLyrics.Tests.Unit/Common/` — base classes resolve the system under test through a real `ServiceCollection`, stored in a field named `sut`.
- NUnit 4 with FluentAssertions throughout; unit tests add NSubstitute, AutoFixture, and `RichardSzalay.MockHttp`.
- `.runsettings` excludes the unit test assembly from coverlet coverage.
- **Don't split suites by `[Category]`** — `Category("Lib")` appears on unit fixtures too, so `--filter "Category!=Lib"` is not a reliable "integration only" switch. Filter by project path instead.

## Gotchas

- **Drive this repo from WSL, not Windows.** Git rejects the repo through the Windows UNC path with a "dubious ownership" error, so both `git` and `dotnet` belong in the WSL shell. Two .NET 10 SDKs exist (WSL 10.0.112, Windows 10.0.400) — build where the checkout lives.
- **Node comes from nvm in WSL.** A bare `node` may be missing while `npm` resolves to the Windows install at `/mnt/c/Program Files/nodejs`. Load nvm and use 22: `. "$NVM_DIR/nvm.sh" && nvm use 22` (WSL has 20/22/24 installed; `engines` requires >= 22).
- **One live-network fixture asserts exact upstream lyrics**, punctuation and all: `CajunLyrics.Tests.Integration/WhenCallingCajunLyricsService.cs`. A failure there means "check whether upstream changed" before assuming a regression in our code.
- **Both controller actions use a bare `catch`**, so every failure surfaces as a generic 500 with no logged exception. Fine for now, but it hides root causes when something does break.
- `.editorconfig` silences CS8604 and CS8602, so nullable-reference warnings will not all surface in the build output even though `Nullable` is enabled everywhere.
