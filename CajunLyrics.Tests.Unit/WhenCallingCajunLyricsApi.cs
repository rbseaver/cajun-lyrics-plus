using AutoFixture;
using CajunLyrics.Lib;
using CajunLyrics.Lib.Http;
using CajunLyrics.Lib.Models;
using CajunLyrics.Lib.Services;
using CajunLyrics.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;
using RichardSzalay.MockHttp;
using System.Net;

namespace CajunLyrics.Tests.Unit
{
    [TestFixture]
    [Category("Lib")]
    public class WhenCallingCajunLyricsApi : LyricsClientTestBase
    {
        private static readonly Dictionary<string, string> XmlResponseHeaders = new()
        {
            { "Content-Type", "text/xml;charset=Off" } // The API returns this content type, which is not valid
        };

        private Fixture fixture;
        private ILyricsService testLyricsService;

        [SetUp]
        public void SetUp()
        {
            fixture = new Fixture();
            testLyricsService = new CajunLyricsService(sut);
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

            var lyricResult = TestLyricUtility.BuildLyricResults(artist, title, true).First();
            var lyricResultXmlString = TestLyricUtility.BuildLyricResultXmlString(lyricResult);

            var responseHeaders = new Dictionary<string, string>
            {
                { "Content-Type", "text/xml; charset=UTF-8" }
            };
            foreach (var language in LyricLanguage.DefaultSearch)
            {
                MockHttpMessageHandler
                    .Expect(
                        HttpMethod.Get,
                        $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf={language.Code}")
                    .Respond(responseHeaders, new StringContent(lyricResultXmlString));
            }

            // Act
            LyricResult result = await testLyricsService.GetSongLyricsAsync(lyricSearchRequest);

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

            IList<LyricResult> lyricResults = TestLyricUtility.BuildLyricResults(artist, title, false, 5);

            var lyricSearchResult = fixture.Build<LyricSearchResult>()
                .With(s => s.LyricResults, lyricResults)
                .Create();
            var lyricSearchResultString = TestLyricUtility.BuildSearchResultXmlString(lyricSearchResult);

            foreach (var language in LyricLanguage.DefaultSearch)
            {
                MockHttpMessageHandler.Expect(
                    HttpMethod.Get,
                    $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}&lf={language.Code}")
                    .Respond(XmlResponseHeaders, new StringContent(lyricSearchResultString));
            }

            // Act
            LyricSearchResult results = await testLyricsService.GetSearchResultsAsync(lyricSearchRequest);

            // Assert
            results.Should().BeEquivalentTo(lyricSearchResult);
        }

        [Test]
        public async Task ShouldGetSearchResultsWithLanguageSpecified()
        {
            // Arrange
            var artist = fixture.Create<string>();
            var title = fixture.Create<string>();
            var language = LyricLanguage.LouisianaFrench;

            var lyricSearchRequest = fixture.Build<LyricSearchRequest>()
                .With(r => r.Artist, artist)
                .With(r => r.Title, title)
                .With(r => r.Languages, new[] { language })
                .Create();

            IList<LyricResult> lyricResults = TestLyricUtility.BuildLyricResults(artist, title, false, 5);

            var lyricSearchResult = fixture.Build<LyricSearchResult>()
                .With(s => s.LyricResults, lyricResults)
                .Create();
            var lyricSearchResultString = TestLyricUtility.BuildSearchResultXmlString(lyricSearchResult);

            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}&lf={language.Code}")
                .Respond(XmlResponseHeaders, new StringContent(lyricSearchResultString));

            // Act
            var results = await testLyricsService.GetSearchResultsAsync(lyricSearchRequest);

            // Assert
            results.Should().BeEquivalentTo(lyricSearchResult);
        }

        [Test]
        public async Task ShouldSearchEveryLanguageWhenAnySpecified()
        {
            // Arrange
            var artist = fixture.Create<string>();
            var title = fixture.Create<string>();

            var lyricSearchRequest = new LyricSearchRequest
            {
                Artist = artist,
                Title = title,
                Languages = [LyricLanguage.Any]
            };

            var lyricSearchResult = fixture.Build<LyricSearchResult>()
                .With(s => s.LyricResults, TestLyricUtility.BuildLyricResults(artist, title, false, 3))
                .Create();
            var lyricSearchResultString = TestLyricUtility.BuildSearchResultXmlString(lyricSearchResult);

            // A single upstream call, without any lf filter
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}")
                .Respond(XmlResponseHeaders, new StringContent(lyricSearchResultString));

            // Act
            var results = await testLyricsService.GetSearchResultsAsync(lyricSearchRequest);

            // Assert
            results.Should().BeEquivalentTo(lyricSearchResult);
            MockHttpMessageHandler.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ShouldMergeSearchResultsAcrossLanguagesAndDedupeById()
        {
            // Arrange
            var artist = fixture.Create<string>();
            var title = fixture.Create<string>();

            var lyricSearchRequest = new LyricSearchRequest
            {
                Artist = artist,
                Title = title,
                Languages = [LyricLanguage.English, LyricLanguage.French]
            };

            var sharedResult = TestLyricUtility.CreateLyricResult(artist, title, null);
            var englishOnly = TestLyricUtility.CreateLyricResult(artist, title, null);
            var frenchOnly = TestLyricUtility.CreateLyricResult(artist, title, null);

            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}&lf=en")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildSearchResultXmlString(
                    new LyricSearchResult { LyricResults = [englishOnly, sharedResult] })));
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}&lf=fr")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildSearchResultXmlString(
                    new LyricSearchResult { LyricResults = [sharedResult, frenchOnly] })));

            // Act
            var results = await testLyricsService.GetSearchResultsAsync(lyricSearchRequest);

            // Assert
            results.LyricResults.Should().BeEquivalentTo([englishOnly, sharedResult, frenchOnly]);
            MockHttpMessageHandler.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ShouldReturnLyricsFromFirstDefaultLanguageThatHasThem()
        {
            // Arrange
            var artist = fixture.Create<string>();
            var title = fixture.Create<string>();

            var lyricSearchRequest = new LyricSearchRequest
            {
                Artist = artist,
                Title = title
            };

            var notFound = TestLyricUtility.CreateLyricResult(artist, title, "Not found");
            var acadianLyric = TestLyricUtility.CreateLyricResult(artist, title, fixture.Create<string>());

            var responseHeaders = new Dictionary<string, string>
            {
                { "Content-Type", "text/xml; charset=UTF-8" }
            };

            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_LA")
                .Respond(responseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(notFound)));
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_AC")
                .Respond(responseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(acadianLyric)));
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_QC")
                .Respond(responseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(notFound)));

            // Act
            var result = await testLyricsService.GetSongLyricsAsync(lyricSearchRequest);

            // Assert
            result.Should().BeEquivalentTo(acadianLyric);
            MockHttpMessageHandler.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ShouldReturnLyricsFromFallbackLanguageWhenOneIsThrottled()
        {
            // Arrange
            var artist = fixture.Create<string>();
            var title = fixture.Create<string>();

            var lyricSearchRequest = new LyricSearchRequest
            {
                Artist = artist,
                Title = title
            };

            var acadianLyric = TestLyricUtility.CreateLyricResult(artist, title, fixture.Create<string>());

            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_LA")
                .Respond(HttpStatusCode.TooManyRequests);
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_AC")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(acadianLyric)));
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_QC")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(acadianLyric)));

            // Act
            var result = await testLyricsService.GetSongLyricsAsync(lyricSearchRequest);

            // Assert
            result.Should().BeEquivalentTo(acadianLyric);
            MockHttpMessageHandler.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ShouldKeepPartialSearchResultsWhenOneLanguageIsThrottled()
        {
            // Arrange
            var artist = fixture.Create<string>();
            var title = fixture.Create<string>();

            var lyricSearchRequest = new LyricSearchRequest
            {
                Artist = artist,
                Title = title
            };

            var expectedResults = fixture.Build<LyricSearchResult>()
                .With(s => s.LyricResults, TestLyricUtility.BuildLyricResults(artist, title, false, 3))
                .Create();

            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}&lf=fr_LA")
                .Respond(HttpStatusCode.TooManyRequests);
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}&lf=fr_AC")
                .Respond(HttpStatusCode.TooManyRequests);
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}&lf=fr_QC")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildSearchResultXmlString(expectedResults)));

            // Act
            var results = await testLyricsService.GetSearchResultsAsync(lyricSearchRequest);

            // Assert
            results.Should().BeEquivalentTo(expectedResults);
        }

        [Test]
        public async Task ShouldRethrowWhenEveryLanguageRequestFails()
        {
            // Arrange
            var artist = fixture.Create<string>();
            var title = fixture.Create<string>();

            var lyricSearchRequest = new LyricSearchRequest
            {
                Artist = artist,
                Title = title
            };

            foreach (var language in LyricLanguage.DefaultSearch)
            {
                MockHttpMessageHandler.Expect(
                    HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}&lf={language.Code}")
                    .Respond(HttpStatusCode.TooManyRequests);
            }

            // Act
            var act = () => testLyricsService.GetSearchResultsAsync(lyricSearchRequest);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Test]
        public async Task ShouldHandleNoSearchResults()
        {
            // Arrange
            var artist = fixture.Create<string>();
            var title = fixture.Create<string>();
            var lyricSearchRequest = new LyricSearchRequest
            {
                Artist = artist,
                Title = title
            };
            var lyricSearchResult = fixture.Build<LyricSearchResult>()
                .With(s => s.LyricResults, [])
                .Create();
            var lyricSearchResultString = TestLyricUtility.BuildSearchResultXmlString(lyricSearchResult);

            foreach (var language in LyricLanguage.DefaultSearch)
            {
                MockHttpMessageHandler.Expect(
                    HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricSearchList.php?artist={artist}&title={title}&lf={language.Code}")
                    .Respond(XmlResponseHeaders, new StringContent(lyricSearchResultString));
            }

            // Act
            var results = await testLyricsService.GetSearchResultsAsync(lyricSearchRequest);

            // Assert
            results.LyricResults.Should().BeEmpty();
        }
    }
}
