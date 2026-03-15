using CajunLyrics.Lib.Models;

namespace CajunLyrics.Lib.Helpers
{
    static class RequestUtilities
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

            // Only include parameters that are not null or empty
            queryParams.AddIfNotNullOrEmpty(request.Artist, "artist");
            queryParams.AddIfNotNullOrEmpty(request.Title, "title");
            queryParams.AddIfNotNullOrEmpty(request.Language, "lf");

            return string.Join('&', queryParams);
        }
    }
}