using CajunLyrics.Lib.Models;

namespace CajunLyrics.Lib.Helpers
{
    static class RequestUtilities
    {
        public static string BuildRequestUri(string resource, int id)
        {
            return $"{resource}?id={id}";
        }

        public static string BuildRequestUri(string resource, LyricSearchRequest request, LyricLanguage? language = null)
        {
            var queryParams = BuildQueryString(request, language);

            var requestUri = $"{resource}?{queryParams}";
            return requestUri;
        }

        private static string BuildQueryString(LyricSearchRequest request, LyricLanguage? language)
        {
            var queryParams = new List<string>();

            // Only include parameters that are not null or empty
            queryParams.AddIfNotNullOrEmpty(request.Artist, "artist");
            queryParams.AddIfNotNullOrEmpty(request.Title, "title");
            queryParams.AddIfNotNullOrEmpty(language?.Code, "lf");

            return string.Join('&', queryParams);
        }
    }
}
