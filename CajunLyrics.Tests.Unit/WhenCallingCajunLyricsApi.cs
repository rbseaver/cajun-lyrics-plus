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
            // Arrange
            var artist = fixture.Create<string>();
            var title = fixture.Create<string>();

            var lyricSearchRequest = new LyricSearchRequest
            {
                Artist = artist,
                Title = title
            };

            var lyricResult = BuildLyricResults(artist, title, true).First();
            var lyricResultXmlString = BuildLyricResultXmlString(lyricResult);
            
            var responseHeaders = new Dictionary<string, string>
            {
                { "Content-Type", "text/xml; charset=UTF-8" }
            };
            MockHttpMessageHandler
                .Expect(
                    HttpMethod.Get,
                    $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}")
                .Respond(responseHeaders, "text/xml", lyricResultXmlString);

            // Act
            LyricResult result = await sut.GetSongLyricsAsync(lyricSearchRequest);

            // Assert
            result.Should().BeEquivalentTo(lyricResult);
        }

        [Test]
        public async Task ShouldGetSearchResults()
        {
            // Arrange
            var artist = fixture.Create<string>();
            var title = fixture.Create<string>();

            var lyricSearchRequest = new LyricSearchRequest
            {
                Artist = artist,
                Title = title
            };

            IList<LyricResult> lyricResults = BuildLyricResults(artist, title, false, 5);

            var lyricSearchResult = fixture.Build<LyricSearchResult>()
                .With(s => s.LyricResults, lyricResults)
                .Create();
            var lyricSearchResultString = BuildSearchResultXmlString(lyricSearchResult);
            var responseHeaders = new Dictionary<string, string>
            {
                { "Content-Type", "text/xml; charset=Off" }
            };
            MockHttpMessageHandler.Expect(
                HttpMethod.Get,
                $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}")
                .Respond(responseHeaders, "text/xml", lyricSearchResultString);

            // Act
            LyricSearchResult results = await sut.GetSearchResultsAsync(lyricSearchRequest);

            // Assert
            results.Should().BeEquivalentTo(lyricSearchResult);
        }

        [Test]
        public async Task ShouldGetSearchResultsWithLanguageSpecified()
        {
            // Arrange
            var artist = fixture.Create<string>();
            var title = fixture.Create<string>();
            var language = fixture.Create<string>();

            var lyricSearchRequest = fixture.Build<LyricSearchRequest>()
                .With(r => r.Artist, artist)
                .With(r => r.Title, title)
                .With(r => r.Language, language)
                .Create();

            IList<LyricResult> lyricResults = BuildLyricResults(artist, title, false, 5);

            var lyricSearchResult = fixture.Build<LyricSearchResult>()
                .With(s => s.LyricResults, lyricResults)
                .Create();
            var lyricSearchResultString = BuildSearchResultXmlString(lyricSearchResult);

            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}&lf={language}")
                .Respond(HttpStatusCode.OK, "text/xml", lyricSearchResultString);

            // Act
            var results = await sut.GetSearchResultsAsync(lyricSearchRequest);

            // Assert
            results.Should().BeEquivalentTo(lyricSearchResult);
        }
    }
}
