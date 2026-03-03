using AutoFixture;
using CajunLyrics.Lib;
using CajunLyrics.Tests.Unit.TestUtilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;


namespace CajunLyrics.Tests.Unit
{
    public class WhenCallingCajunLyricsApi : TestBase
    {
        private CajunLyricsService cajunLyricsService;
        private ServiceCollection services;
        private Fixture fixture;

        [SetUp]
        public void SetUp()
        {
            fixture = new Fixture();
            services = new ServiceCollection();
            services.AddSingleton(MockHttpClientFactory);
            services.AddTransient<CajunLyricsService>();
            var provider = services.BuildServiceProvider();

            cajunLyricsService = provider.GetRequiredService<CajunLyricsService>();
        }

        [Test]
        public async Task ShouldGetLyricsAsLyricResult()
        {
            MockHttpMessageHandler
                .Expect(
                    HttpMethod.Get,
                    $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={TestData.Artist[0]}&title={TestData.Title[0]}")
                .Respond(HttpStatusCode.OK, "text/xml", TestData.ExpectedLyricResult);

            LyricResult result = await cajunLyricsService.GetSongLyricsAsync(TestData.Artist[0], TestData.Title[0]);

            result.Should()
                .BeEquivalentTo(
                    new LyricResult
                    {
                        Id = 2891,
                        Artist = TestData.Artist[0],
                        Title = TestData.Title[0],
                        Lyric = TestData.Lyric,
                        LyricsUrl = TestData.LyricsUrl[0].Replace("&amp;","&"),
                        ArtistUrl = TestData.ArtistUrl[0].Replace("&amp;", "&"),
                    },
                    options => options.ExcludingMissingMembers());

            MockHttpMessageHandler.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ShouldGetSearchResults()
        {
            var artist = TestData.Artist[0];
            var title = TestData.Title[0];

            MockHttpMessageHandler.Expect(
                HttpMethod.Get,
                $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}")
                .Respond(HttpStatusCode.OK, "text/xml", TestData.ExpectedSearchResult);

            LyricSearchResult results = await cajunLyricsService.GetSearchResultsAsync(artist, title);

            results.Should()
                .BeEquivalentTo(
                    new LyricSearchResult
                    {
                        LyricResults =
                            [
                                    new()
                                    {
                                        Id = TestData.Id[0],
                                        Artist = TestData.Artist[0],
                                        Title = TestData.Title[0],
                                        LyricsUrl = TestData.LyricsUrl[0].Replace("&amp;", "&"),
                                        ArtistUrl = TestData.ArtistUrl[0].Replace("&amp;", "&")
                                    },
                                    new()
                                    {
                                        Id = TestData.Id[1],
                                        Artist = TestData.Artist[1],
                                        Title = TestData.Title[1],
                                        LyricsUrl = TestData.LyricsUrl[1].Replace("&amp;", "&"),
                                        ArtistUrl = TestData.ArtistUrl[1].Replace("&amp;", "&")
                                    }
                                ]
                    });
        }
    }
}
