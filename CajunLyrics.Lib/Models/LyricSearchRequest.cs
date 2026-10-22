using System;

namespace CajunLyrics.Lib.Models
{
    public class LyricSearchRequest
    {
        public string? Artist { get; set; }
        public string? Title { get; set; }

        /// <summary>
        /// Languages to search. When null or empty, the service searches
        /// <see cref="LyricLanguage.DefaultSearch"/>; <see cref="LyricLanguage.Any"/>
        /// disables the language filter.
        /// </summary>
        public IReadOnlyList<LyricLanguage>? Languages { get; set; }
    }
}
