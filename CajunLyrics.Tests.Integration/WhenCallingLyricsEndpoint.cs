using System.Net;
using System.Net.Http.Json;
using CajunLyrics.Lib.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

[assembly: Parallelizable(ParallelScope.None)]

namespace CajunLyrics.Tests.Integration;

[TestFixture]
[Category("Integration")]
[NonParallelizable]
public class WhenCallingLyricsEndpoint
{
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        factory = new WebApplicationFactory<Program>();
        client = factory.CreateClient();
    }

    [SetUp]
    public async Task SetUp()
    {
        // Avoid bursting upstream third-party API in rapid succession
        await Task.Delay(1000);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        client.Dispose();
        factory.Dispose();
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
            using var productionFactory = factory.WithWebHostBuilder(builder =>
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
    }

    [Test]
    public async Task ShouldReturn404WhenSongNotFound()
    {
        var response = await client.GetAsync("/api/Lyrics/lyric?artist=NonExistentArtist9999&title=NonExistentTitle9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
        var noRedirectClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await noRedirectClient.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().Be(new Uri("/scalar/v1", UriKind.Relative));
    }
}
