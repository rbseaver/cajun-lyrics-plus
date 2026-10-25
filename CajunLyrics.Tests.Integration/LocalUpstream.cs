using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace CajunLyrics.Tests.Integration;

/// <summary>
/// A local WireMock stand-in for api.cajunlyrics.com, serving a recorded
/// snapshot of real upstream responses (see <c>UpstreamFixtures/</c>) so the
/// suite runs hermetically: no network, no rate limit, no pacing between
/// tests. Also hands out the shared <see cref="WebApplicationFactory{Program}"/>,
/// whose <c>HttpClient:BaseAddress</c> points at the stub.
/// </summary>
[SetUpFixture]
[Category("Integration")]
public sealed class LocalUpstream
{
    private static WireMockServer server = null!;
    private static WebApplicationFactory<Program> factory = null!;

    /// <summary>Base URL of the stub, trailing slash included.</summary>
    public static string BaseUrl { get; private set; } = null!;

    /// <summary>The API under test, configured to call the stub.</summary>
    public static WebApplicationFactory<Program> Factory => factory;

    [OneTimeSetUp]
    public void Start()
    {
        server = WireMockServer.Start();
        BaseUrl = $"{server.Urls[0]}/";

        RegisterDirectSearch();
        RegisterSearch();
        RegisterFaultScenarios();

        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["HttpClient:BaseAddress"] = BaseUrl
                })));
    }

    [OneTimeTearDown]
    public void Stop()
    {
        factory.Dispose();
        server.Stop();
        server.Dispose();
    }

    /// <summary>
    /// Every mapping pins artist, title and language exactly, except where
    /// upstream answers all languages identically — then one mapping without a
    /// language filter covers them. A given query must never mix both styles:
    /// when two mappings match, only registration order decides, and specific
    /// mappings are registered first (the search catch-all last) so a pinned
    /// mapping always wins.
    /// </summary>
    private static void RegisterDirectSearch()
    {
        // The known song: the fr_LA filter carries it, the other filters have
        // something else (or nothing) for the same query.
        ServeLyric("feufollet", "Blues de Dix Ans", "fr_LA", "direct-blues-fr_LA.xml");
        ServeLyric("feufollet", "Blues de Dix Ans", "fr_AC", "direct-blues-fr_AC.xml");
        ServeLyric("feufollet", "Blues de Dix Ans", "fr_QC", "direct-blues-fr_QC.xml");

        // Upstream stores this title with a bracketed annotation the query
        // does not carry: "Travailler c'est trop dur [Cap Enragé]".
        ServeLyric("Zachary Richard", "Travailler C'est Trop Dur", "fr_LA", "direct-travailler-fr_LA.xml");

        // Fayo's song is tagged Acadian upstream; the fr_LA and fr_QC filters
        // fall back to matching the word "moonshine" inside other songs' lyrics.
        ServeLyric("Fayo", "Moonshine", "fr_LA", "direct-moonshine-fr_LA.xml");
        ServeLyric("Fayo", "Moonshine", "fr_AC", "direct-moonshine-fr_AC.xml");
        ServeLyric("Fayo", "Moonshine", "fr_QC", "direct-moonshine-fr_QC.xml");

        ServeLyricById("1054", "direct-by-id.xml");

        ServeNotFound("NonExistentArtist9999", "NonExistentTitle9999");
    }

    private static void RegisterSearch()
    {
        ServeSearch("feufollet", "Blues de Dix Ans", "fr_LA", "search-blues-fr_LA.xml");
        ServeSearch("feufollet", "Blues de Dix Ans", "fr_AC", "search-blues-fr_AC.xml");
        ServeSearch("feufollet", "Blues de Dix Ans", "fr_QC", "search-blues-fr_QC.xml");

        ServeSearch("Zachary Richard", null, "fr_LA", "search-zachary-fr_LA.xml");
        ServeSearch("Zachary Richard", null, "en", "search-zachary-en.xml");

        // "Any language" resolves to a single request without a filter. It has
        // to be registered after the two mappings above: same query otherwise,
        // and only their absence of a match on this one request keeps them
        // unambiguous.
        ServeSearch("Zachary Richard", null, null, "search-zachary.xml");
    }

    private static void RegisterFaultScenarios()
    {
        // The fr_LA filter trips the upstream rate limit; the fan-out must keep
        // going and serve the song from fr_AC instead.
        server
            .Given(Request.Create()
                .WithPath("/LyricDirectSearch.php")
                .WithParam("artist", "ThrottleTest")
                .WithParam("title", "ServeMeNext")
                .WithParam("lf", "fr_LA"))
            .RespondWith(Response.Create().WithStatusCode(429));
        ServeLyric("ThrottleTest", "ServeMeNext", "fr_AC", "stub-throttle-song.xml");
        ServeLyric("ThrottleTest", "ServeMeNext", "fr_QC", "not-found.xml");

        // Upstream entirely unavailable: every language fails, so the request
        // must surface as an error rather than a wrong song or a 404.
        server
            .Given(Request.Create()
                .WithPath("/LyricDirectSearch.php")
                .WithParam("artist", "BrokenUpstream")
                .WithParam("title", "NothingWorks"))
            .RespondWith(Response.Create().WithStatusCode(503));
    }

    private static void ServeLyric(string artist, string title, string languageCode, string fixture)
    {
        server
            .Given(Request.Create()
                .WithPath("/LyricDirectSearch.php")
                .WithParam("artist", artist)
                .WithParam("title", title)
                .WithParam("lf", languageCode))
            .RespondWith(XmlBody(fixture));
    }

    private static void ServeNotFound(string artist, string title)
    {
        // No language filter: upstream answers every language the same way
        // when the song does not exist.
        server
            .Given(Request.Create()
                .WithPath("/LyricDirectSearch.php")
                .WithParam("artist", artist)
                .WithParam("title", title))
            .RespondWith(XmlBody("not-found.xml"));
    }

    private static void ServeLyricById(string id, string fixture)
    {
        server
            .Given(Request.Create()
                .WithPath("/LyricDirectSearch.php")
                .WithParam("id", id))
            .RespondWith(XmlBody(fixture));
    }

    private static void ServeSearch(string artist, string? title, string? languageCode, string fixture)
    {
        var request = Request.Create()
            .WithPath("/LyricSearchList.php")
            .WithParam("artist", artist);

        if (title is not null)
        {
            request = request.WithParam("title", title);
        }

        if (languageCode is not null)
        {
            request = request.WithParam("lf", languageCode);
        }

        server.Given(request).RespondWith(XmlBody(fixture));
    }

    private static IResponseBuilder XmlBody(string fixture) =>
        Response.Create()
            .WithStatusCode(200)
            .WithHeader("Content-Type", "text/xml; charset=UTF-8")
            .WithBodyFromFile(Path.Combine(AppContext.BaseDirectory, "UpstreamFixtures", fixture));
}
