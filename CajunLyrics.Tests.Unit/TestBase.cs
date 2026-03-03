using AutoFixture;
using NSubstitute;
using RichardSzalay.MockHttp;

namespace CajunLyrics.Tests.Unit
{
    public class TestBase
    {
        readonly private Fixture baseFixture;
        readonly private IHttpClientFactory httpClientFactoryMock;
        readonly private HttpClient mockHttpClient;
        readonly private MockHttpMessageHandler mockHttpMessageHandler;
        private readonly Uri uri;

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