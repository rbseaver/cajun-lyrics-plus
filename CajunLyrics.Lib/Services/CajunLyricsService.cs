using CajunLyrics.Lib.Helpers;
using CajunLyrics.Lib.Models;
using System.Text;
using System.Xml.Serialization;

namespace CajunLyrics.Lib.Services
{
    public class CajunLyricsService(IHttpClientFactory httpClientFactory)
: ILyricsService
    {
        private static readonly Dictionary<Type, XmlSerializer> Serializers = new()
        {
            { typeof(LyricResult), new XmlSerializer(typeof(LyricResult)) },
            { typeof(LyricSearchResult), new XmlSerializer(typeof(LyricSearchResult)) }
        };

        public async Task<LyricResult> GetSongLyricsAsync(LyricSearchRequest request)
        {
            return await DeserializeResultsAsync<LyricResult>("LyricDirectSearch.php", request);
        }

        public async Task<LyricSearchResult> GetSearchResultsAsync(LyricSearchRequest request)
        {
            return await DeserializeResultsAsync<LyricSearchResult>("LyricSearchList.php", request);
        }

        private async Task<T> DeserializeResultsAsync<T>(string resource, LyricSearchRequest request)
        {
            var xml = await FetchXmlResponse(resource, request);

            using var reader = new StringReader(xml);

            return Serializers[typeof(T)].Deserialize(reader) is T result ? result :
                throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name} from XML");
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