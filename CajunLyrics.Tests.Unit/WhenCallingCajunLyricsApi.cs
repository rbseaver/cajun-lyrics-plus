using AutoFixture;
using CajunLyrics.Lib;
using CajunLyrics.Lib.Models;
using FluentAssertions;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text;
using System.Web;

namespace CajunLyrics.Tests.Unit
{
    public class WhenCallingCajunLyricsApi : TestBase
    {
        private Fixture fixture;

        [SetUp]
        public void SetUp()
        {
            fixture = new Fixture();
        }

        [Test]
        public async Task ShouldGetLyricsAsLyricResult()
        {
            var artist = fixture.Create<string>();
            var title = fixture.Create<string>();
            var lyricResult = BuildLyricResults(artist, title, true).First();
            var lyricResultXmlString = BuildLyricResultXmlString(lyricResult);
            MockHttpMessageHandler
                .Expect(
                    HttpMethod.Get,
                    $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}")
                .Respond(HttpStatusCode.OK, "text/xml", lyricResultXmlString);

            LyricResult result = await sut.GetSongLyricsAsync(artist, title);

            result.Should().BeEquivalentTo(lyricResult);
        }

        [Test]
        public async Task ShouldGetSearchResults()
        {
            var artist = fixture.Create<string>();
            var title = fixture.Create<string>();
            IList<LyricResult> lyricResults = BuildLyricResults(artist, title, false, 5);
            var lyricSearchResult = fixture.Build<LyricSearchResult>()
                .With(s => s.LyricResults, lyricResults)
                .Create();
            var lyricSearchResultString = BuildSearchResultXmlString(lyricSearchResult);
            MockHttpMessageHandler.Expect(
                HttpMethod.Get,
                $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}")
                .Respond(HttpStatusCode.OK, "text/xml", lyricSearchResultString);

            LyricSearchResult results = await sut.GetSearchResultsAsync(artist, title);

            results.Should().BeEquivalentTo(lyricSearchResult);
        }
    }
}
