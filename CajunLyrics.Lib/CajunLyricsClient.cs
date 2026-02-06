using System.Xml.Serialization;

namespace CajunLyrics.Lib
{
    public class CajunLyricsClient(IHttpClientFactory httpClientFactory)
    {
        private readonly IHttpClientFactory httpClientFactory = httpClientFactory;

        public async Task<LyricResult> GetSongLyricsAsync(string artist, string title)
        {
            var client = httpClientFactory.CreateClient(nameof(CajunLyricsClient));
            var response = await client.GetAsync($"LyricDirectSearch.php?artist={artist}&title={title}");

            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync();
            var serializer = new XmlSerializer(typeof(LyricResult));

            using var reader = new StringReader(xml);

            var result = serializer.Deserialize(reader) as LyricResult ??
                throw new InvalidOperationException("Failed to deserialize LyricResult from XML.");

            return result;
        }
    }
}