using AutoFixture;
using CajunLyrics.Lib;
using CajunLyrics.Lib.Models;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RichardSzalay.MockHttp;
using System.Text;
using System.Web;

namespace CajunLyrics.Tests.Unit
{
    public class TestBase
    {
        readonly private Fixture baseFixture;
        readonly private IHttpClientFactory httpClientFactoryMock;
        readonly private HttpClient mockHttpClient;
        readonly private MockHttpMessageHandler mockHttpMessageHandler;
        private readonly Uri uri;
        protected readonly CajunLyricsService sut;
        private readonly ServiceCollection services;

        public TestBase()
        {
            baseFixture = new Fixture();
            uri = baseFixture.Create<Uri>();

            mockHttpMessageHandler = new MockHttpMessageHandler();
            mockHttpClient = new HttpClient(mockHttpMessageHandler)
            {
                BaseAddress = new Uri(BaseUrl)
            };

            httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            httpClientFactoryMock.CreateClient(Arg.Any<string>()).Returns(mockHttpClient);

            services = new ServiceCollection();
            services.AddSingleton(MockHttpClientFactory);
            services.AddTransient<CajunLyricsService>();
            var provider = services.BuildServiceProvider();

            sut = provider.GetRequiredService<CajunLyricsService>();
        }

        public string BaseUrl
        {
            get
            {
                return uri.AbsoluteUri;
            }
        }

        public HttpClient MockHttpClient
        {
            get
            {
                return mockHttpClient;
            }
        }

        public IHttpClientFactory MockHttpClientFactory
        {
            get
            {
                return httpClientFactoryMock;
            }
        }

        public MockHttpMessageHandler MockHttpMessageHandler
        {
            get
            {
                return mockHttpMessageHandler;
            }
        }

        protected IList<LyricResult> BuildLyricResults(string artist, string title, int count = 1)
        {
            IList<LyricResult> lyricResults = [
                baseFixture.Build<LyricResult>()
                .With(l => l.Id, baseFixture.Create<int>())
                .With(l => l.LyricsUrl, $"{baseFixture.Create<Uri>()}?lyrics={baseFixture.Create<int>()}")
                .With(l => l.ArtistUrl, $"{baseFixture.Create<Uri>()}?page=search&amp;artist={baseFixture.Create<int>()}")
                .With(l => l.Artist, artist)
                .With(l => l.Title, title)
                .With(l => l.Lyric, () => null)
                .Create()
            ];

            if (count > 1)
            {
                for (int i = 1; i < count; i++)
                {
                    lyricResults.Add(
                        baseFixture.Build<LyricResult>()
                        .With(l => l.Id, baseFixture.Create<int>())
                        .With(l => l.LyricsUrl, $"{baseFixture.Create<Uri>()}?lyrics={baseFixture.Create<int>()}")
                        .With(l => l.ArtistUrl, $"{baseFixture.Create<Uri>()}?page=search&amp;artist={baseFixture.Create<int>()}")
                        .With(l => l.Artist, artist)
                        .With(l => l.Title, title)
                        .With(l => l.Lyric, () => null)
                        .Create());
                }
            }
            return lyricResults;
        }

        protected static string BuildLyricResultXmlString(LyricResult result)
        {
            return @$"<?xml version='1.0' encoding='UTF-8'?>
<GetLyricResult xmlns='http://api.cajunlyrics.com/'>
    <Id>{result.Id}</Id>
    <LyricsUrl>{HttpUtility.HtmlEncode(result.LyricsUrl)}</LyricsUrl>
    <ArtistUrl>{HttpUtility.HtmlEncode(result.ArtistUrl)}</ArtistUrl>
    <Artist>{result.Artist}</Artist>
    <Title>{result.Title}</Title>
    <Lyric>{result.Lyric}</Lyric>
</GetLyricResult>
";
        }

        protected static string BuildSearchResultXmlString(LyricSearchResult searchResult)
        {
            var header = @$"<?xml version='1.0' encoding='UTF-8'?><SearchLyricResultArray xmlns='http://api.cajunlyrics.com/'>";
            var body = new StringBuilder();
            foreach (var lyricResult in searchResult.LyricResults)
            {
                body.Append($@"<SearchLyricsResult>
<Id>{lyricResult.Id}</Id>
<LyricsUrl>{HttpUtility.HtmlEncode(lyricResult.LyricsUrl)}</LyricsUrl>
<ArtistUrl>{HttpUtility.HtmlEncode(lyricResult.ArtistUrl)}</ArtistUrl>
<Artist>{lyricResult.Artist}</Artist>
<Title>{lyricResult.Title}</Title>");
                body.Append("</SearchLyricsResult>");

            }
            var footer = "</SearchLyricResultArray>";
            return header + body.ToString() + footer;
        }
    }

    public class Artist(Fixture fixture)
    {
        readonly Fixture fixture = fixture;

        public string FirstName
        {
            get
            {
                return fixture.Create<string>();
            }
        }

        public string LastName
        {
            get
            {
                return fixture.Create<string>();
            }
        }
    }
}