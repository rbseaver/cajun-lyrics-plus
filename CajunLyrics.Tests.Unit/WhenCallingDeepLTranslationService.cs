using CajunLyrics.Lib.Configuration;
using CajunLyrics.Lib.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;

namespace CajunLyrics.Tests.Unit;

[TestFixture]
[Category("Lib")]
public class WhenCallingDeepLTranslationService
{
    [Test]
    public async Task ShouldSendSelectedTextToDeepLForEnglishTranslation()
    {
        var handler = new MockHttpMessageHandler();
        handler.Expect(HttpMethod.Post, "https://api-free.deepl.com/v2/translate")
            .WithHeaders("Authorization", "DeepL-Auth-Key test-key")
            .WithPartialContent("\"target_lang\":\"EN\"")
            .WithPartialContent("\"text\":[\"Bonjour, mon ami\"]")
            .Respond("application/json", "{\"translations\":[{\"text\":\"Hello, my friend\"}]}");
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-free.deepl.com")
        };
        var service = new DeepLTranslationService(
            httpClient,
            Options.Create(new DeepLOptions { ApiKey = "test-key" }));

        var translation = await service.TranslateToEnglishAsync("Bonjour, mon ami");

        translation.Should().Be("Hello, my friend");
        handler.VerifyNoOutstandingExpectation();
    }

    [Test]
    public async Task ShouldNotCallDeepLWhenApiKeyIsMissing()
    {
        var handler = new MockHttpMessageHandler();
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api-free.deepl.com")
        };
        var service = new DeepLTranslationService(
            httpClient,
            Options.Create(new DeepLOptions()));

        var act = () => service.TranslateToEnglishAsync("Bonjour");

        await act.Should().ThrowAsync<TranslationNotConfiguredException>();
    }
}
