using AutoFixture;
using CajunLyrics.Lib.Http;
using CajunLyrics.Tests.Unit.Common;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RichardSzalay.MockHttp;
using System.Net;

namespace CajunLyrics.Tests.Unit;

public class WhenCallingCajunLyricsClient : LyricsClientTestBase
{

    private ILyricsClient cajunLyricsClient;
    private IFixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = new Fixture();
        MockHttpClientFactory.CreateClient(Arg.Any<string>()).Returns(MockHttpClient);
        cajunLyricsClient = new CajunLyricsClient(MockHttpClientFactory);
    }

    [Test]
    public async Task ShouldSendAndHandleSuccessStatusCode()
    {
        MockHttpMessageHandler.Expect(HttpMethod.Get, MockHttpClient.BaseAddress.ToString())
            .Respond(HttpStatusCode.OK);

        var response = await cajunLyricsClient.GetLyricsAsync(MockHttpClient.BaseAddress);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        MockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Test]
    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task ShouldHandleNonSuccessStatusCodes(HttpStatusCode expectedStatusCode)
    {
        MockHttpMessageHandler.Expect(HttpMethod.Get, MockHttpClient.BaseAddress.ToString())
            .Respond(expectedStatusCode);
        var response = await cajunLyricsClient.GetLyricsAsync(MockHttpClient.BaseAddress);

        response.StatusCode.Should().Be(expectedStatusCode);
        MockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Test]
    public async Task ShouldParsePlainTextContent()
    {
        MockHttpMessageHandler.Expect(HttpMethod.Get, MockHttpClient.BaseAddress.ToString())
            .Respond("text/plain", "Test content");

        var response = await cajunLyricsClient.GetLyricsAsync(MockHttpClient.BaseAddress);

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Be("Test content");
        MockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Test]
    public async Task ShouldParseJsonContent()
    {
        var responseHeaders = new Dictionary<string, string>
            {
                { "Content-Type", "application/json; charset=UTF-8" }
            };

        MockHttpMessageHandler.Expect(HttpMethod.Get, MockHttpClient.BaseAddress.ToString())
            .Respond(responseHeaders, new StringContent("{\"key\":\"value\"}"));

        var response = await cajunLyricsClient.GetLyricsAsync(MockHttpClient.BaseAddress);
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Be("{\"key\":\"value\"}");
        MockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Test]
    public async Task ShouldParseXmlContent()
    {
        var responseHeaders = new Dictionary<string, string>
            {
                { "Content-Type", "text/xml; charset=UTF-8" }
            };
        MockHttpMessageHandler.Expect(HttpMethod.Get, MockHttpClient.BaseAddress.ToString())
            .Respond(responseHeaders, new StringContent("<root><key>value</key></root>"));

        var response = await cajunLyricsClient.GetLyricsAsync(MockHttpClient.BaseAddress);
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Be("<root><key>value</key></root>");
        MockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Test]
    public async Task ShouldHandleUnexpectedError()
    {
        MockHttpClientFactory.CreateClient(Arg.Any<string>()).Throws(new InvalidOperationException("Failed to create HttpClient instance."));

        Func<Task> requestAction = async() => await cajunLyricsClient.GetLyricsAsync(MockHttpClient.BaseAddress);

        await requestAction.Should().ThrowAsync<HttpRequestException>()
            .WithMessage($"An unspecified error occurred when trying to initialize the request: Failed to create HttpClient instance.");
        MockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }
}
