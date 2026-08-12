namespace CajunLyrics.Lib.Http
{
    public interface ILyricsClient
    {
        Task<HttpResponseMessage> GetLyricsAsync(Uri uri);
    }
}
