using CajunLyrics.Lib.Http;
using CajunLyrics.Lib.Models;
using CajunLyrics.Lib.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CajunLyrics.Tests.Integration;

/// <summary>
/// The live tier: a thin canary against the real upstream, excluded from CI
/// (filter on <c>Category!=Live</c>) and earmarked for CD — a failure at
/// deploy time means investigate, starting with upstream drift. If one of
/// these fails, the recorded corpus in <c>UpstreamFixtures/</c> is what needs
/// refreshing, not the hermetic tests.
/// </summary>
[TestFixture]
[Category("Live")]
[Category("Integration")]
[NonParallelizable]
public class WhenCallingUpstreamLive
{
    private ILyricsService cajunLyricsService = null!;

    [SetUp]
    public async Task SetUp()
    {
        // The upstream API rate-limits at roughly 6 requests per 10 seconds per IP,
        // and the default language search fans out to 3 requests — back off enough
        // between tests to stay inside the window.
        await Task.Delay(4000);

        var services = new ServiceCollection();
        services.AddHttpClient(nameof(CajunLyricsClient), (sp, client) =>
        {
            client.BaseAddress = new Uri("https://api.cajunlyrics.com/");
        });
        services.AddScoped<ILyricsClient, CajunLyricsClient>();
        services.AddScoped<ILyricsService, CajunLyricsService>();
        cajunLyricsService = services.BuildServiceProvider().GetRequiredService<ILyricsService>();
    }

    [Test]
    public async Task ShouldServeTheRequestedSongFromTheLiveUpstream()
    {
        // Regression: the fr_LA filter answers this query with a different song
        // whose lyrics merely contain "moonshine" — the live upstream must still
        // be answerable with the requested song itself.
        var result = await cajunLyricsService.GetSongLyricsAsync(
            new LyricSearchRequest { Artist = "Fayo", Title = "Moonshine" });

        result.Artist.Should().Be("Fayo");
        result.Title.Should().Be("Moonshine");
        result.HasLyric.Should().BeTrue();
    }

    [Test]
    public async Task ShouldStillReturnTheKnownSongTheCorpusWasRecordedFrom()
    {
        var result = await cajunLyricsService.GetSongLyricsAsync(
            new LyricSearchRequest { Artist = "feufollet", Title = "Blues de Dix Ans" });

        result.Id.Should().Be(512,
            "if upstream changed this song, the recorded corpus in UpstreamFixtures/ is stale");
        result.Artist.Should().Be("Feufollet");
        result.HasLyric.Should().BeTrue();
    }
}
