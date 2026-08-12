using AutoFixture;
using CajunLyrics.Lib;
using CajunLyrics.Lib.Configuration;
using CajunLyrics.Lib.Http;
using CajunLyrics.Lib.Models;
using CajunLyrics.Lib.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RichardSzalay.MockHttp;
using System.Text;
using System.Web;

namespace CajunLyrics.Tests.Unit.Common
{
    public class LyricsClientTestBase
    {
        readonly private Fixture baseFixture;
        readonly private IHttpClientFactory httpClientFactoryMock;
        readonly private HttpClient mockHttpClient;
        readonly private MockHttpMessageHandler mockHttpMessageHandler;
        private readonly Uri uri;
        protected readonly ILyricsClient sut;
        private readonly ServiceCollection services;

        public LyricsClientTestBase()
        {
            baseFixture = new Fixture();
            uri = baseFixture.Create<Uri>();
            var options = new HttpClientOptions
            {
                BaseAddress = uri
            };
            mockHttpMessageHandler = new MockHttpMessageHandler();
            mockHttpClient = new HttpClient(mockHttpMessageHandler)
            {
                BaseAddress = options.BaseAddress
            };

            httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            httpClientFactoryMock.CreateClient(Arg.Any<string>()).Returns(mockHttpClient);

            services = new ServiceCollection();
            services.AddSingleton(MockHttpClientFactory);
            services.AddTransient<ILyricsClient, CajunLyricsClient>();
            var provider = services.BuildServiceProvider();

            sut = provider.GetRequiredService<ILyricsClient>();
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
    }
}