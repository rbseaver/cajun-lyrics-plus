using CajunLyrics.Lib;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RichardSzalay.MockHttp;
using System.Net;


namespace CajunLyrics.Tests.Unit
{
    public class WhenCallingCajunLyricsApi
    {
        private ServiceCollection? services;
        private IHttpClientFactory? httpClientFactoryMock;
        private CajunLyricsClient? cajunLyricsClient;
        private HttpClient? mockHttpClient;
        private MockHttpMessageHandler? mockHttpMessageHandler;
        private const string BaseUrl = "https://api.derp.duh/";

        private const string ExpectedLyricResult = @"<?xml version='1.0' encoding='UTF-8'?>
<GetLyricResult xmlns='http://api.cajunlyrics.com/'>
    <Id>2891</Id>
    <LyricsUrl>http://www.cajunlyrics.com/?lyrics=2891</LyricsUrl>
    <ArtistUrl>http://www.cajunlyrics.com/?page=search&amp;artist=168</ArtistUrl>
    <Artist>Dewey Balfa</Artist>
    <Title>J’ai Pleuré</Title>
    <Lyric>Moi, j’ai pleuré équand toi t’as parti
        J’ai pleuré parce que j't’aimais
        T’as pris mon sentiment
        Et mon 'tit brin d’agrément
        Tu m’as laissé, moi, tout seul dans l’abandon

        [Instrumental]

        Tu voudras, ’tit monde, toi, t’en r'venir
        Avec moi là à la maison, mais c’est là, tu vas 'oir
        Tu vas pleurer, pareil comment j’ai fait
        Ça s'ra trop tard, moi j'pourras pas t'pardonner

        Lyrics Provided by CajunLyrics.com
    </Lyric>
</GetLyricResult>
";
        private const string ExpectedSearchResult = @"<?xml version='1.0' encoding='UTF-8'?>
<SearchLyricResultArray xmlns=""http://api.cajunlyrics.com/"">
  <SearchLyricsResult>
    <Id> ID (RESULT #1) </Id>
    <LyricsUrl> Lyrics Page URL</LyricsUrl>
    <ArtistUrl> Artist Page URL </ArtistUrl>
    <Artist> Artist Name </Artist>
    <Title> Song Title </Title>
  </SearchLyricsResult>
  <SearchLyricsResult>
    <Id> ID (RESULT #2) </Id>
    <LyricsUrl> Lyrics Page URL</LyricsUrl>
    <ArtistUrl> Artist Page URL </ArtistUrl>
    <Artist> Artist Name </Artist>
    <Title> Song Title </Title>
  </SearchLyricsResult>
<SearchLyricResultArray>";

        [SetUp]
        public void SetUp()
        {
            mockHttpMessageHandler = new MockHttpMessageHandler();
            mockHttpClient = new HttpClient(mockHttpMessageHandler)
            {
                BaseAddress = new Uri(BaseUrl)
            };

            httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            httpClientFactoryMock.CreateClient(Arg.Any<string>()).Returns(mockHttpClient);

            services = new ServiceCollection();
            services.AddSingleton(httpClientFactoryMock);
            services.AddTransient<CajunLyricsClient>();
            var provider = services.BuildServiceProvider();

            cajunLyricsClient = provider.GetRequiredService<CajunLyricsClient>();
        }

        [Test]
        public async Task ShouldMakeCallToDirectLyricSearch()
        {
            mockHttpMessageHandler
                .Expect(HttpMethod.Get, $"{BaseUrl}LyricDirectSearch.php?artist=Dewey%20Balfa&title=J%27ai%20Pleur%C3%A9")
                .Respond(HttpStatusCode.OK, "text/xml", ExpectedLyricResult);

            await cajunLyricsClient.GetSongLyricsAsync("Dewey Balfa", "J'ai Pleuré");

            mockHttpMessageHandler.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ShouldGetLyricsAsLyricResult()
        {
            mockHttpMessageHandler
                .Expect(HttpMethod.Get, $"{BaseUrl}LyricDirectSearch.php?artist=Dewey%20Balfa&title=J%27ai%20Pleur%C3%A9")
                .Respond(HttpStatusCode.OK, "text/xml", ExpectedLyricResult);

            LyricResult response = await cajunLyricsClient.GetSongLyricsAsync("Dewey Balfa", "J'ai Pleuré");

            response.Id.Should().Be(2891);
            response.Artist.Should().Be("Dewey Balfa");
            response.Title.Should().Be("J’ai Pleuré");
            response.Lyric.Should().Contain("Moi, j’ai pleuré équand toi t’as parti");
        }

        [Test]
        public async Task ShouldMakeCallToSearch()
        {
            mockHttpMessageHandler.Expect(HttpMethod.Get, $"{BaseUrl}LyricSearchList.php?artist=ArtistName")
                .Respond(HttpStatusCode.OK, "text/xml", ExpectedSearchResult);
        }

        [TearDown]
        public void TearDown()
        {
            mockHttpClient.Dispose();
            mockHttpMessageHandler.Dispose();
        }
    }
}
