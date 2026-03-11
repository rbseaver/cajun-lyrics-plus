using CajunLyrics.Lib.Models;
using System.Xml.Serialization;

namespace CajunLyrics.Lib
{
    public class CajunLyricsService(IHttpClientFactory httpClientFactory)
    {
        private const string LyricResultEndpoint = "LyricDirectSearch.php";
        private const string LyricSearchResultsEndpoint = "LyricSearchList.php";

        private readonly IHttpClientFactory httpClientFactory = httpClientFactory;

        public async Task<LyricResult> GetSongLyricsAsync(string artist, string title)
        {
            var xml = await FetchXmlResponse(LyricResultEndpoint, artist, title);

            var serializer = new XmlSerializer(typeof(LyricResult));

            using var reader = new StringReader(xml);

            var result = serializer.Deserialize(reader) as LyricResult ??
                throw new InvalidOperationException("Failed to deserialize LyricResult from XML.");

            return result;
        }
        public async Task<LyricSearchResult> GetSearchResultsAsync(string artist, string title)
        {
            var xml = await FetchXmlResponse(LyricSearchResultsEndpoint, artist, title);

            var serializer = new XmlSerializer(typeof(LyricSearchResult));

            using var reader = new StringReader(xml);

            var result = serializer.Deserialize(reader) as LyricSearchResult ??
                throw new InvalidOperationException("Failed do deserialize LyricSearchResult from XML");

            return result;
        }

        private async Task<string> FetchXmlResponse(string endpointUri, string artist, string title)
        {
            var client = httpClientFactory.CreateClient(nameof(CajunLyricsService));
            
            var response = await client.GetAsync($"{endpointUri}?artist={artist}&title={title}");

            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync();

            return xml;
        }
    }
}