# Architecture

Two apps in one repo: **CajunLyrics.Api** (C# / ASP.NET Core on .NET 10) and **CajunLyrics.Web** (Vite + React 19 + TypeScript), deployed as a single Docker container. The API wraps the external CajunLyrics.com service, which answers in **XML** — responses are fetched as bytes, decoded UTF-8, and deserialized with `XmlSerializer` before the API returns JSON to the browser.

## Project layout and dependency direction

```mermaid
flowchart TB
    Web["CajunLyrics.Web<br/>React SPA (Vite)"]
    Api["CajunLyrics.Api<br/>controllers, Program.cs, static files"]
    Lib["CajunLyrics.Lib<br/>services, http, configuration, models"]
    Unit["CajunLyrics.Tests.Unit"]
    Integ["CajunLyrics.Tests.Integration"]

    Api -->|"references — the only direction allowed"| Lib
    Unit --> Api
    Unit --> Lib
    Integ --> Api
```

`CajunLyrics.Lib` never references `CajunLyrics.Api`; controllers stay thin and delegate to `ILyricService`. The SPA is independent — the API serves its built output as static files with an `index.html` fallback, which is what makes the single-container deployment possible.

## Request flow (Docker / production)

```mermaid
flowchart LR
    B["Browser"]
    subgraph C["Single container — docker compose up"]
        S["Static SPA files<br/>(wwwroot)"]
        A["ASP.NET Core API<br/>(Kestrel)"]
    end
    U["api.cajunlyrics.com<br/>(XML)"]

    B -->|"GET /"| S
    B -->|"GET /api/Lyrics/..."| A
    A -->|"HTTPS request"| U
    U -->|"XML bytes"| A
    A -->|"JSON response"| B
```

The SPA and `/api` share one origin (port 8080 in Docker), so the browser never sees CORS or the upstream URL.

## Local development wiring

`npm run dev` (repo root) starts both processes together with `concurrently`:

```mermaid
flowchart LR
    B["Browser<br/>:5173"]
    V["Vite dev server<br/>(HMR)"]
    A["ASP.NET API<br/>(dotnet run, :7156)"]
    U["api.cajunlyrics.com<br/>(XML)"]

    B -->|"app + assets"| V
    V -->|"/api/* proxied"| A
    A -->|"HTTPS"| U
```

Vite proxies `/api` to `https://localhost:7156`, accepting the self-signed dev cert — `vite preview` shares the same proxy config. Only the frontend gets hot reload; `dotnet run` does not watch (swap in `dotnet watch` if you want that).

## Testing tiers

- **Unit (NUnit)** — `CajunLyrics.Tests.Unit`, no network. Part of the CI gate.
- **Hermetic integration** — `CajunLyrics.Tests.Integration` runs the real API against a local WireMock stub serving recorded upstream responses (`UpstreamFixtures/`). Part of the CI gate (`integration_tests` job, `TestCategory!=Live`).
- **Live canary** — `Category("Live")` hits the real CajunLyrics.com API on purpose. Excluded from CI, reserved for CD/deploy-time checks: a failure there means upstream changed and the recorded corpus needs refreshing.
