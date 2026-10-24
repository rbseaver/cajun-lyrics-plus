using CajunLyrics.Lib.Models;
using System.Globalization;
using System.Text;

namespace CajunLyrics.Lib.Helpers
{
    static class LyricResultExtensions
    {
        /// <summary>
        /// True when the upstream result actually is the song the request asked
        /// for. The upstream direct search is fuzzy — it happily answers with a
        /// different song that merely contains the requested title somewhere in
        /// its lyrics — so artist and title are compared word by word after
        /// normalization (case, punctuation, diacritics): every requested word
        /// must appear in the answer or vice versa. Fields the request left
        /// empty are not checked at all.
        /// </summary>
        public static bool Matches(this LyricResult result, LyricSearchRequest request)
        {
            return MatchesField(request.Artist, result.Artist)
                && MatchesField(request.Title, result.Title);
        }

        private static bool MatchesField(string? requested, string? actual)
        {
            if (string.IsNullOrWhiteSpace(requested))
            {
                return true;
            }

            var requestedTokens = Tokenize(requested);
            var actualTokens = Tokenize(actual);

            // Either side may carry extra words — upstream annotates some titles
            // ("Travailler c'est trop dur [Cap Enragé]") and users may type more
            // than upstream stores — but a song whose words have nothing in
            // common with the request is a different song.
            return requestedTokens.Length > 0
                && actualTokens.Length > 0
                && (requestedTokens.All(actualTokens.Contains) || actualTokens.All(requestedTokens.Contains));
        }

        private static string[] Tokenize(string? value)
        {
            return Normalize(value).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Reduces a name or title to comparable letters and digits: decomposed
        /// to strip diacritics, lowercased, with every run of other characters
        /// collapsed into a single space. "Drunkard’s" and "drunkard's" both
        /// normalize to "drunkard s".
        /// </summary>
        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
            var normalized = new StringBuilder(decomposed.Length);
            var pendingSeparator = false;

            foreach (var character in decomposed)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category is UnicodeCategory.NonSpacingMark
                    or UnicodeCategory.SpacingCombiningMark
                    or UnicodeCategory.EnclosingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(character))
                {
                    if (pendingSeparator && normalized.Length > 0)
                    {
                        normalized.Append(' ');
                    }

                    pendingSeparator = false;
                    normalized.Append(char.ToLowerInvariant(character));
                }
                else
                {
                    pendingSeparator = true;
                }
            }

            return normalized.ToString();
        }
    }
}
