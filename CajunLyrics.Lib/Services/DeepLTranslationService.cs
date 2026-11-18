using CajunLyrics.Lib.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CajunLyrics.Lib.Services;

public sealed class DeepLTranslationService(
    HttpClient httpClient,
    IOptions<DeepLOptions> options) : ITranslationService
{
    public async Task<string> TranslateToEnglishAsync(string text, CancellationToken cancellationToken = default)
    {
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new TranslationNotConfiguredException();
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v2/translate");
        request.Headers.TryAddWithoutValidation("Authorization", $"DeepL-Auth-Key {apiKey}");
        request.Content = JsonContent.Create(new DeepLRequest([text], "EN"));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<DeepLResponse>(cancellationToken);
        var translation = payload?.Translations?.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(translation))
        {
            throw new HttpRequestException("DeepL returned an empty translation response.");
        }

        return translation;
    }

    private sealed record DeepLRequest(
        [property: JsonPropertyName("text")] string[] Text,
        [property: JsonPropertyName("target_lang")] string TargetLanguage);

    private sealed record DeepLResponse(
        [property: JsonPropertyName("translations")] List<DeepLTranslatedText>? Translations);

    private sealed record DeepLTranslatedText(
        [property: JsonPropertyName("text")] string? Text);
}
