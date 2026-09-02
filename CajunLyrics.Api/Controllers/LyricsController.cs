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
        public async Task<IActionResult> Get([FromQuery] string artist, [FromQuery] string title, [FromQuery] LyricLanguage? language)
        {
            var request = new LyricSearchRequest
            {
                Artist = artist,
                Title = title,
                Language = language
            };
            try
            {
                var result = await cajunLyricsService.GetSongLyricsAsync(request);
                if (string.IsNullOrWhiteSpace(result.Lyric) || string.Equals(result.Lyric.Trim(), "Not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound($"Lyrics not found for '{artist}' and '{title}'");
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
        public async Task<IActionResult> Search([FromQuery] string? artist, [FromQuery] string? title, [FromQuery] LyricLanguage? language)
        {
            var request = new LyricSearchRequest
            {
                Artist = artist,
                Title = title,
                Language = language
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
    }
}
