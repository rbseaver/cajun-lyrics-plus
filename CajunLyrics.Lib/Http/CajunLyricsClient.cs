using System.Net;

namespace CajunLyrics.Lib.Http
{
    public class CajunLyricsClient(IHttpClientFactory httpClientFactory) : ILyricsClient
    {
        public async Task<HttpResponseMessage> GetLyricsAsync(Uri uri)
        {
            try
            {
                var client = httpClientFactory.CreateClient(nameof(CajunLyricsClient));
                var response = await client.GetAsync(uri);
                return response;
            }
            catch (InvalidOperationException ex)
            {
                throw new HttpRequestException($"An unspecified error occurred when trying to initialize the request: {ex.Message}", ex);
            }
        }
    }
}
