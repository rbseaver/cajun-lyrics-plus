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
        public async Task<IActionResult> Get([FromQuery] string artist, [FromQuery] string title, [FromQuery] string language)
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
                if (string.IsNullOrEmpty(result.Lyric))
                {
                    return NotFound($"Lyrics not found for '{artist}' and '{title}");
                }
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "An error occurred while attempting to retrieve lyrics.");
            }
        }
    }
}
