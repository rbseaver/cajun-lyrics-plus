using CajunLyrics.Lib.Models;

namespace CajunLyrics.Lib.Helpers
{
    internal static class RequestUtilities
    {
        public static string BuildRequestUri(string resource, LyricSearchRequest request)
        {
            var queryParams = BuildQueryString(request);

            var requestUri = $"{resource}?{queryParams}";
            return requestUri;
        }

        private static string BuildQueryString(LyricSearchRequest request)
        {
            var queryParams = new List<string>
            {
                $"artist={request.Artist}",
                $"title={request.Title}"
            };

            if (request.Language != null)
            {
                queryParams.Add($"lf={request.Language}");
            }
            ;

            return string.Join('&', queryParams);
        }
    }
}