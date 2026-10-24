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
            // All three default languages answer with this lyric, so the first
            // (Louisiana French) is the one that matches.
            lyricResult.Language = LyricLanguage.LouisianaFrench;
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
            result.Language.Should().Be(LyricLanguage.LouisianaFrench);
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
            acadianLyric.Language = LyricLanguage.AcadianFrench;

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
            result.Language.Should().Be(LyricLanguage.AcadianFrench);
            MockHttpMessageHandler.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ShouldNotStampLanguageWhenSearchingAnyLanguage()
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

            var lyricResult = TestLyricUtility.CreateLyricResult(artist, title, fixture.Create<string>());

            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(lyricResult)));

            // Act
            var result = await testLyricsService.GetSongLyricsAsync(lyricSearchRequest);

            // Assert
            result.Language.Should().BeNull();
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
            acadianLyric.Language = LyricLanguage.AcadianFrench;

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
        public async Task ShouldSkipResultsThatAreNotTheRequestedSong()
        {
            // Arrange
            var artist = "Fayo";
            var title = "Moonshine";

            var lyricSearchRequest = new LyricSearchRequest
            {
                Artist = artist,
                Title = title
            };

            // Upstream's direct search is fuzzy: the fr_LA filter answers with a
            // different song whose lyrics merely contain the word "moonshine".
            var contentMatch = TestLyricUtility.CreateLyricResult("Robert Bertrand", "Drunkard's Two Step", fixture.Create<string>());
            var expected = TestLyricUtility.CreateLyricResult(artist, title, fixture.Create<string>());
            expected.Language = LyricLanguage.AcadianFrench;
            var notFound = TestLyricUtility.CreateLyricResult(artist, title, "Not found");

            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_LA")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(contentMatch)));
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_AC")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(expected)));
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_QC")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(notFound)));

            // Act
            var result = await testLyricsService.GetSongLyricsAsync(lyricSearchRequest);

            // Assert
            result.Should().BeEquivalentTo(expected);
            result.Language.Should().Be(LyricLanguage.AcadianFrench);
            MockHttpMessageHandler.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ShouldReturnNoLyricWhenNoLanguageAnswersTheRequestedSong()
        {
            // Arrange
            var artist = "Fayo";
            var title = "Moonshine";

            var lyricSearchRequest = new LyricSearchRequest
            {
                Artist = artist,
                Title = title
            };

            // Every filter answers with some other song that does have lyrics.
            var first = TestLyricUtility.CreateLyricResult("Robert Bertrand", "Drunkard's Two Step", fixture.Create<string>());
            var second = TestLyricUtility.CreateLyricResult("Suroît", "La Lianne", fixture.Create<string>());
            var third = TestLyricUtility.CreateLyricResult("Zachary Richard", "High Time", fixture.Create<string>());

            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_LA")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(first)));
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_AC")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(second)));
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_QC")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(third)));

            // Act
            var result = await testLyricsService.GetSongLyricsAsync(lyricSearchRequest);

            // Assert
            result.HasLyric.Should().BeFalse("the endpoint must answer 404 rather than serve another song");
            MockHttpMessageHandler.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ShouldMatchTheRequestedSongDespiteCasingAndApostropheDifferences()
        {
            // Arrange
            var artist = "ROBERT BERTRAND";
            var title = "drunkard's two step";

            var lyricSearchRequest = new LyricSearchRequest
            {
                Artist = artist,
                Title = title
            };

            // Upstream stores the title with a typographic apostrophe and title casing.
            var upstreamResult = TestLyricUtility.CreateLyricResult("Robert Bertrand", "Drunkard’s Two Step", fixture.Create<string>());
            upstreamResult.Language = LyricLanguage.LouisianaFrench;
            var notFound = TestLyricUtility.CreateLyricResult(artist, title, "Not found");

            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_LA")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(upstreamResult)));
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_AC")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(notFound)));
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_QC")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(notFound)));

            // Act
            var result = await testLyricsService.GetSongLyricsAsync(lyricSearchRequest);

            // Assert
            result.Should().BeEquivalentTo(upstreamResult);
            result.Language.Should().Be(LyricLanguage.LouisianaFrench);
            MockHttpMessageHandler.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ShouldMatchWhenUpstreamAnnotatesTheTitle()
        {
            // Arrange
            var artist = "Zachary Richard";
            var title = "Travailler C'est Trop Dur";

            var lyricSearchRequest = new LyricSearchRequest
            {
                Artist = artist,
                Title = title
            };

            // Upstream stores the title with a bracketed annotation the query
            // does not carry.
            var upstreamResult = TestLyricUtility.CreateLyricResult(artist, "Travailler c’est trop dur [Cap Enragé]", fixture.Create<string>());
            upstreamResult.Language = LyricLanguage.LouisianaFrench;
            var notFound = TestLyricUtility.CreateLyricResult(artist, title, "Not found");

            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_LA")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(upstreamResult)));
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_AC")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(notFound)));
            MockHttpMessageHandler.Expect(
                HttpMethod.Get, $"{MockHttpClient.BaseAddress}LyricDirectSearch.php?artist={artist}&title={title}&lf=fr_QC")
                .Respond(XmlResponseHeaders, new StringContent(TestLyricUtility.BuildLyricResultXmlString(notFound)));

            // Act
            var result = await testLyricsService.GetSongLyricsAsync(lyricSearchRequest);

            // Assert
            result.Should().BeEquivalentTo(upstreamResult);
            result.Language.Should().Be(LyricLanguage.LouisianaFrench);
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
