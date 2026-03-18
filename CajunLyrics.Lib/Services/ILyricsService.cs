using CajunLyrics.Lib.Models;

namespace CajunLyrics.Lib.Services
{
    public interface ILyricsService
    {
        Task<LyricSearchResult> GetSearchResultsAsync(LyricSearchRequest request);
        Task<LyricResult> GetSongLyricsAsync(LyricSearchRequest request);
    }
}