# Hermetic integration tests (WireMock) — done, 2026-10-24

Implemented and green: unit 56/56, integration 18/18 (16 hermetic + 2 live).
Hermetic run takes ~4s (was ~56s of rate-limit pacing).

## What landed

- **WireMock.Net 2.20.0** in `CajunLyrics.Tests.Integration`; `LocalUpstream`
  (`[SetUpFixture]`) starts the stub and a shared `WebApplicationFactory` with
  `HttpClient:BaseAddress` pointed at it — no production code changed.
- **`UpstreamFixtures/`** — recorded snapshot of real upstream responses
  (16 captures, paced ~2s to respect the ~6 req/10s limit) plus two hand-made
  fault fixtures. The not-found payload is identical for every filter upstream,
  so one `not-found.xml` serves four mappings.
- **Hermetic suite**: endpoint fixture (14 tests) + service fixture (2), no
  pacing. Includes two fault tests that were impossible live: 429 on one
  language → fan-out falls back to the next; every language failing → 500.
  `ShouldPointTheApiAtTheLocalUpstreamStub` guards that the config override
  really took (otherwise tests would silently drift back to the real upstream).
- **Live tier**: `WhenCallingUpstreamLive.cs`, `Category("Live")` — two canaries
  (the Fayo wrong-song regression + "the known song the corpus came from").
  Excluded from CI via `--filter "TestCategory!=Live"`; earmarked for CD.
- **CI**: new `integration_tests` job; `release_gate` now needs it.
  Docs updated (AGENTS.md, README, copilot-instructions).

## Mapping conventions (read before adding fixtures)

- Pin artist + title + `lf` on every mapping, except where upstream answers all
  languages identically — then register ONE mapping without `lf`. Never mix
  both styles for the same query: when two mappings match, registration order
  is the only tiebreaker, so specific mappings register first (the
  "any language" search catch-all goes last).
- Fault scenarios: `ThrottleTest/ServeMeNext` (fr_LA → 429, fr_AC → stub song,
  fr_QC → not found), `BrokenUpstream/NothingWorks` (all → 503).

## Re-recording the corpus (when a live canary fails)

1. Pace ~2s between requests; use `https://` directly (HTTP→HTTPS redirect
   doubles the count against the rate limit).
2. The exact URL list lives in `LocalUpstream.RegisterDirectSearch` /
   `RegisterSearch` — re-curl each into `UpstreamFixtures/<name>.xml`.
3. `diff` old vs new; if upstream renamed or re-annotated a title, check
   `LyricResultExtensions.Matches` still accepts it (word containment, not
   equality — see the `[Cap Enragé]` case).

## Deferred

- CD wiring for the live tier, when a deployment exists.
