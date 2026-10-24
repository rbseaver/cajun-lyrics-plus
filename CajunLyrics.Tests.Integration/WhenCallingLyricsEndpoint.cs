using System.Net;
using System.Net.Http.Json;
using CajunLyrics.Lib.Configuration;
using CajunLyrics.Lib.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: Parallelizable(ParallelScope.None)]

namespace CajunLyrics.Tests.Integration;

[TestFixture]
[Category("Integration")]
[NonParallelizable]
public class WhenCallingLyricsEndpoint
{
    private HttpClient client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        client = LocalUpstream.Factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        client.Dispose();
    }

    [Test]
    public void ShouldPointTheApiAtTheLocalUpstreamStub()
    {
        var options = LocalUpstream.Factory.Services
            .GetRequiredService<IOptions<HttpClientOptions>>().Value;

        options.BaseAddress.Should().Be(new Uri(LocalUpstream.BaseUrl),
            "the suite must never talk to the real upstream — that is the whole point of the stub");
    }

    [Test]
    public async Task ShouldReturnHealthyFromHealthProbe()
    {
        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Test]
    public async Task ShouldNotRedirectToHttpsOutsideDevelopment()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_HTTPS_PORT", "443");
        try
        {
            using var productionFactory = LocalUpstream.Factory.WithWebHostBuilder(builder =>
                builder.UseEnvironment("Production"));
            using var productionClient = productionFactory.CreateClient(
                new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var response = await productionClient.GetAsync("/");

            response.Headers.Location.Should().BeNull(
                "only Development may issue an HTTPS redirect");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_HTTPS_PORT", null);
        }
    }

    [Test]
    public async Task ShouldReturnLyricResultForKnownSong()
    {
        var response = await client.GetAsync("/api/Lyrics/lyric?artist=feufollet&title=Blues%20de%20Dix%20Ans");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LyricResult>();
        result.Should().NotBeNull();
        result!.Artist.Should().Be("Feufollet");
        result.Title.Should().Be("Blues De Dix Ans");
        result.Id.Should().Be(512);
        result.Lyric.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task ShouldReturnLyricResultWithLanguageFilter()
    {
        var response = await client.GetAsync("/api/Lyrics/lyric?artist=Zachary%20Richard&title=Travailler%20C%27est%20Trop%20Dur&language=fr_LA");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: content);
        var result = await response.Content.ReadFromJsonAsync<LyricResult>();
        result.Should().NotBeNull();
        result!.Artist.Should().Be("Zachary Richard");
        result.Language.Should().NotBeNull();
        result.Language!.Code.Should().Be("fr_LA");
    }

    [Test]
    public async Task ShouldNotServeADifferentSongThanRequested()
    {
        // Regression: upstream's direct search answers the fr_LA filter for this
        // song with a different lyric that merely contains "moonshine" in its
        // text, so the response must be checked against the requested song.
        var response = await client.GetAsync("/api/Lyrics/lyric?artist=Fayo&title=Moonshine");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: content);
        var result = await response.Content.ReadFromJsonAsync<LyricResult>();
        result.Should().NotBeNull();
        result!.Artist.Should().Be("Fayo");
        result.Title.Should().Be("Moonshine");
        result.Lyric.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task ShouldFallBackToTheNextLanguageWhenUpstreamThrottles()
    {
        // The fr_LA filter answers 429 (recorded rate-limit behavior); fr_AC
        // carries the song, so the fan-out must continue past the failure.
        var response = await client.GetAsync("/api/Lyrics/lyric?artist=ThrottleTest&title=ServeMeNext");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: content);
        var result = await response.Content.ReadFromJsonAsync<LyricResult>();
        result.Should().NotBeNull();
        result!.Artist.Should().Be("ThrottleTest");
        result.Language.Should().NotBeNull();
        result.Language!.Code.Should().Be("fr_AC");
    }

    [Test]
    public async Task ShouldSurfaceAnErrorWhenEveryLanguageFails()
    {
        var response = await client.GetAsync("/api/Lyrics/lyric?artist=BrokenUpstream&title=NothingWorks");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError,
            "an unreachable upstream is an error, not a wrong song and not a 404");
    }

    [Test]
    public async Task ShouldReturn404WhenSongNotFound()
    {
        var response = await client.GetAsync("/api/Lyrics/lyric?artist=NonExistentArtist9999&title=NonExistentTitle9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldSearchEveryLanguageWhenAllSpecified()
    {
        var response = await client.GetAsync("/api/Lyrics/search?artist=Zachary%20Richard&language=all");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: content);
        content.Should().Contain("Zachary Richard");
    }

    [Test]
    public async Task ShouldMergeSearchResultsAcrossRequestedLanguages()
    {
        var response = await client.GetAsync("/api/Lyrics/search?artist=Zachary%20Richard&language=fr_LA&language=en");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: content);
        content.Should().Contain("Zachary Richard");
    }

    [Test]
    public async Task ShouldServeOpenApiSpecification()
    {
        var response = await client.GetAsync("/openapi/v1.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("/api/Lyrics/lyric");
    }

    [Test]
    public async Task ShouldServeScalarDocumentation()
    {
        var response = await client.GetAsync("/scalar/v1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldRedirectRootToScalarDocumentation()
    {
        var noRedirectClient = LocalUpstream.Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await noRedirectClient.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().Be(new Uri("/scalar/v1", UriKind.Relative));
    }
}
