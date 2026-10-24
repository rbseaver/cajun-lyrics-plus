using CajunLyrics.Lib.Helpers;
using CajunLyrics.Lib.Http;
using CajunLyrics.Lib.Models;
using System.Text;
using System.Xml.Serialization;

namespace CajunLyrics.Lib.Services
{
    public class CajunLyricsService(ILyricsClient lyricsClient)
: ILyricsService
    {
        private static readonly Dictionary<Type, XmlSerializer> Serializers = new()
        {
            { typeof(LyricResult), new XmlSerializer(typeof(LyricResult)) },
            { typeof(LyricSearchResult), new XmlSerializer(typeof(LyricSearchResult)) }
        };

        public async Task<LyricResult> GetSongLyricsAsync(LyricSearchRequest request)
        {
            var languages = ResolveLanguages(request);

            var results = await FetchAvailableResultsAsync<LyricResult>("LyricDirectSearch.php", request, languages);

            foreach (var (language, result) in results)
            {
                // The upstream direct search is fuzzy: it can answer with a
                // completely different song that merely contains the title in
                // its lyrics. Only a result that actually is the requested song
                // may answer for it.
                if (result.HasLyric && result.Matches(request))
                {
                    // The upstream payload carries no language, but the filter that
                    // matched it does — surface that so the UI can show it.
                    result.Language = language;
                    return result;
                }
            }

            // No filter answered with the requested song. Return a lyric-less
            // result so the endpoint responds 404 instead of serving another song.
            return new LyricResult
            {
                Artist = request.Artist,
                Title = request.Title
            };
        }

        public async Task<LyricSearchResult> GetSearchResultsAsync(LyricSearchRequest request)
        {
            var languages = ResolveLanguages(request);

            var results = await FetchAvailableResultsAsync<LyricSearchResult>("LyricSearchList.php", request, languages);

            return new LyricSearchResult
            {
                LyricResults = results
                    .SelectMany(entry => entry.Result.LyricResults ?? [])
                    .DistinctBy(result => result.Id)
                    .ToList()
            };
        }

        /// <summary>
        /// The upstream API accepts a single language per request, so multi-language
        /// searches resolve one request per language and the results are merged.
        /// An empty selection falls back to <see cref="LyricLanguage.DefaultSearch"/>;
        /// <see cref="LyricLanguage.Any"/> resolves to a single unfiltered request.
        /// </summary>
        private static IReadOnlyList<LyricLanguage?> ResolveLanguages(LyricSearchRequest request)
        {
            var languages = request.Languages?
                .DistinctBy(language => language.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (languages is null || languages.Count == 0)
            {
                return LyricLanguage.DefaultSearch;
            }

            if (languages.Any(language => language == LyricLanguage.Any))
            {
                return [null];
            }

            return languages;
        }

        /// <summary>
        /// Requests the given languages one at a time — the upstream rate-limits per
        /// IP, so bursts are avoided — and tolerates individual failures: a throttled
        /// or failing language contributes nothing rather than failing the whole
        /// search. Only when every request fails is the last error rethrown. Each
        /// result is paired with the language filter that produced it (null when
        /// unfiltered).
        /// </summary>
        private async Task<List<(LyricLanguage? Language, T Result)>> FetchAvailableResultsAsync<T>(string resource, LyricSearchRequest request, IReadOnlyList<LyricLanguage?> languages)
        {
            var results = new List<(LyricLanguage?, T)>();
            HttpRequestException? lastError = null;

            foreach (var language in languages)
            {
                try
                {
                    results.Add((language, await FetchResultAsync<T>(resource, request, language)));
                }
                catch (HttpRequestException ex)
                {
                    lastError = ex;
                }
            }

            if (results.Count == 0 && lastError is not null)
            {
                throw lastError;
            }

            return results;
        }

        private async Task<T> FetchResultAsync<T>(string resource, LyricSearchRequest request, LyricLanguage? language)
        {
            var xml = await FetchXmlResponse(resource, request, language);

            using var reader = new StringReader(xml);

            return Serializers[typeof(T)].Deserialize(reader) is T result ? result :
                throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name} from XML");
        }

        private async Task<string> FetchXmlResponse(string resource, LyricSearchRequest request, LyricLanguage? language)
        {
            string requestUri = RequestUtilities.BuildRequestUri(resource, request, language);

            var response = await lyricsClient.GetLyricsAsync(new Uri(requestUri, UriKind.Relative));

            response.EnsureSuccessStatusCode();

            var bytes = await response.Content.ReadAsByteArrayAsync();

            var xml = Encoding.UTF8.GetString(bytes);

            return xml;
        }
    }
}
