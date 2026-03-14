using CajunLyrics.Lib.Helpers;
using CajunLyrics.Lib.Models;
using System.Text;
using System.Xml.Serialization;

namespace CajunLyrics.Lib.Services
{
    public class CajunLyricsService(IHttpClientFactory httpClientFactory)
    {
        private readonly IHttpClientFactory httpClientFactory = httpClientFactory;

        public async Task<LyricResult> GetSongLyricsAsync(LyricSearchRequest request)
        {
            var xml = await FetchXmlResponse("LyricDirectSearch.php", request);

            var serializer = new XmlSerializer(typeof(LyricResult));

            using var reader = new StringReader(xml);

            var result = serializer.Deserialize(reader) as LyricResult ??
                throw new InvalidOperationException("Failed to deserialize LyricResult from XML.");

            return result;
        }
        public async Task<LyricSearchResult> GetSearchResultsAsync(LyricSearchRequest request)
        {
            var xml = await FetchXmlResponse("LyricSearchList.php", request);

            var serializer = new XmlSerializer(typeof(LyricSearchResult));

            using var reader = new StringReader(xml);

            var result = serializer.Deserialize(reader) as LyricSearchResult ??
                throw new InvalidOperationException("Failed to deserialize LyricSearchResult from XML");

            return result;
        }

        private async Task<string> FetchXmlResponse(string resource, LyricSearchRequest request)
        {
            var client = httpClientFactory.CreateClient(nameof(CajunLyricsService));

            string requestUri = RequestUtilities.BuildRequestUri(resource, request);
          
            var response = await client.GetAsync(requestUri);
            
            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync();

            var xml = Encoding.UTF8.GetString(bytes);

            return xml;
        }
    }
}