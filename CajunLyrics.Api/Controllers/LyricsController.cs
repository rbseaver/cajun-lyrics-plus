using CajunLyrics.Lib.Models;
using CajunLyrics.Lib.Services;
using Microsoft.AspNetCore.Mvc;

namespace CajunLyrics.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LyricsController(ILyricsService cajunLyricsService) : ControllerBase
    {
        [HttpGet]
        [Route("lyric")]
        public async Task<IActionResult> Get([FromQuery] string? artist, [FromQuery] string? title, [FromQuery] List<LyricLanguage>? language, [FromQuery] int? id = null)
        {
            if (id.HasValue ? id.Value <= 0 : string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Provide a positive lyric ID or both a non-empty artist and title.");
            }

            if (!HasValidLanguages(language))
            {
                return BadRequest($"Provide only supported language filters, with no more than {LyricLanguage.All.Count} entries.");
            }

            var request = new LyricSearchRequest
            {
                Artist = artist,
                Title = title,
                Languages = language
            };
            try
            {
                var result = id.HasValue
                    ? await cajunLyricsService.GetSongLyricsByIdAsync(id.Value, language)
                    : await cajunLyricsService.GetSongLyricsAsync(request);
                if (!result.HasLyric)
                {
                    return id.HasValue
                        ? NotFound($"Lyrics not found for ID '{id}'")
                        : NotFound($"Lyrics not found for '{artist}' and '{title}'");
                }
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "An error occurred while attempting to retrieve lyrics.");
            }
        }

        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> Search([FromQuery] string? artist, [FromQuery] string? title, [FromQuery] List<LyricLanguage>? language)
        {
            if (string.IsNullOrWhiteSpace(artist) && string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Provide an artist, a title, or both.");
            }

            if (!HasValidLanguages(language))
            {
                return BadRequest($"Provide only supported language filters, with no more than {LyricLanguage.All.Count} entries.");
            }

            var request = new LyricSearchRequest
            {
                Artist = artist,
                Title = title,
                Languages = language
            };

            try
            {
                var result = await cajunLyricsService.GetSearchResultsAsync(request);
                if (result.LyricResults is null || result.LyricResults.Count == 0)
                {
                    return NotFound("No matching lyrics found for that search.");
                }

                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "An error occurred while attempting to search lyrics.");
            }
        }

        private static bool HasValidLanguages(IReadOnlyCollection<LyricLanguage>? languages)
        {
            return languages is null
                || (languages.Count <= LyricLanguage.All.Count
                    && languages.All(language => language is not null
                        && LyricLanguage.All.Any(supported =>
                            string.Equals(supported.Code, language.Code, StringComparison.OrdinalIgnoreCase))));
        }
    }
}
