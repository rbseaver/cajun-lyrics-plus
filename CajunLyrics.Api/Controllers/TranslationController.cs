using CajunLyrics.Lib.Models;
using CajunLyrics.Lib.Services;
using Microsoft.AspNetCore.Mvc;

namespace CajunLyrics.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class TranslationController(
    ITranslationService translationService,
    ILogger<TranslationController> logger) : ControllerBase
{
    private const int MaximumTextLength = 5000;

    [HttpPost("translate")]
    public async Task<IActionResult> Translate(
        [FromBody] TranslationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Provide text to translate.");
        }

        if (request.Text.Length > MaximumTextLength)
        {
            return BadRequest($"Text must not exceed {MaximumTextLength} characters.");
        }

        try
        {
            var translation = await translationService.TranslateToEnglishAsync(request.Text, cancellationToken);
            return Ok(new TranslationResponse(translation));
        }
        catch (TranslationNotConfiguredException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Translation is not configured.");
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "DeepL translation request failed.");
            return StatusCode(StatusCodes.Status502BadGateway, "The translation service is unavailable.");
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception, "DeepL translation request timed out.");
            return StatusCode(StatusCodes.Status502BadGateway, "The translation service is unavailable.");
        }
    }
}
