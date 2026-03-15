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
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(request.Artist))
            {
                queryParams.Add($"artist={request.Artist}");
            }

            if (!string.IsNullOrEmpty(request.Title))
            {
                queryParams.Add($"title={request.Title}");
            }

            if (!string.IsNullOrEmpty(request.Language))
            {
                queryParams.Add($"lf={request.Language}");
            }

            return string.Join('&', queryParams);
        }
    }
}