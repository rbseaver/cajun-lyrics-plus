using CajunLyrics.Lib.Models;
using System.Xml.Serialization;

namespace CajunLyrics.Lib
{
    public class CajunLyricsService(IHttpClientFactory httpClientFactory)
    {
        private readonly IHttpClientFactory httpClientFactory = httpClientFactory;

        public async Task<LyricResult> GetSongLyricsAsync(string artist, string title)
        {
            var client = httpClientFactory.CreateClient(nameof(CajunLyricsService));
            var response = await client.GetAsync($"LyricDirectSearch.php?artist={artist}&title={title}");

            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync();
            var serializer = new XmlSerializer(typeof(LyricResult));

            using var reader = new StringReader(xml);

            var result = serializer.Deserialize(reader) as LyricResult ??
                throw new InvalidOperationException("Failed to deserialize LyricResult from XML.");

            return result;
        }
        public async Task<LyricSearchResult> GetSearchResultsAsync(string artist, string title)
        {
            var client = httpClientFactory.CreateClient(nameof(CajunLyricsService));
            var response = await client.GetAsync($"LyricSearchList.php?artist={artist}&title={title}");

            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync();
            var serializer = new XmlSerializer(typeof(LyricSearchResult));

            using var reader = new StringReader(xml);

            var result = serializer.Deserialize(reader) as LyricSearchResult ??
                throw new InvalidOperationException("Failed do deserialize LyricSearchResult from XML");

            return result;
        }
    }
}