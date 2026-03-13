using CajunLyrics.Lib.Models;
using System.Xml.Serialization;

namespace CajunLyrics.Lib
{
    public class CajunLyricsService(IHttpClientFactory httpClientFactory)
    {
        private const string LyricResultEndpoint = "LyricDirectSearch.php";
        private const string LyricSearchResultsEndpoint = "LyricSearchList.php";

        private readonly IHttpClientFactory httpClientFactory = httpClientFactory;

        public async Task<LyricResult> GetSongLyricsAsync(LyricSearchRequest request)
        {
            var xml = await FetchXmlResponse(LyricResultEndpoint, request);

            var serializer = new XmlSerializer(typeof(LyricResult));

            using var reader = new StringReader(xml);

            var result = serializer.Deserialize(reader) as LyricResult ??
                throw new InvalidOperationException("Failed to deserialize LyricResult from XML.");

            return result;
        }
        public async Task<LyricSearchResult> GetSearchResultsAsync(LyricSearchRequest request)
        {
            var xml = await FetchXmlResponse(LyricSearchResultsEndpoint, request);

            var serializer = new XmlSerializer(typeof(LyricSearchResult));

            using var reader = new StringReader(xml);

            var result = serializer.Deserialize(reader) as LyricSearchResult ??
                throw new InvalidOperationException("Failed to deserialize LyricSearchResult from XML");

            return result;
        }

        private async Task<string> FetchXmlResponse(string resource, LyricSearchRequest request)
        {
            var client = httpClientFactory.CreateClient(nameof(CajunLyricsService));

            var queryParams = new List<string>
            {
                $"artist={request.Artist}",
                $"title={request.Title}"
            };

            if (request.Language != null)
            {
                queryParams.Add($"lf={request.Language}");
            };

            var requestUri = $"{resource}?{string.Join("&", queryParams)}";

            var response = await client.GetAsync(requestUri);

            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync();

            return xml;
        }
    }
}