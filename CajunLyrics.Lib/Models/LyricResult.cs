using System.Xml.Serialization;

namespace CajunLyrics.Lib.Models
{
    [XmlRoot("GetLyricResult", Namespace = "http://api.cajunlyrics.com/")]
    public class LyricResult
    {
        [XmlIgnore]
        public int Id { get; set; }

        [XmlElement("Id")]
        public string? IdString
        {
            get => Id.ToString();
            set => Id = int.TryParse(value, out var id) ? id : 0;
        }

        [XmlElement("LyricsUrl")]
        public string? LyricsUrl { get; set; }

        [XmlElement("ArtistUrl")]
        public string? ArtistUrl { get; set; }

        [XmlElement("Artist")]
        public string? Artist { get; set; }

        [XmlElement("Title")]
        public string? Title { get; set; }

        [XmlElement("Lyric")]
        public string? Lyric { get; set; }

        /// <summary>
        /// The language filter that matched this lyric during a multi-language
        /// search. Not part of the upstream payload — stamped by the service.
        /// Null when the lyric was found without a language filter.
        /// </summary>
        [XmlIgnore]
        public LyricLanguage? Language { get; set; }

        /// <summary>
        /// True when the response contains an actual lyric rather than an
        /// empty body or the upstream "Not found" marker.
        /// </summary>
        [XmlIgnore]
        public bool HasLyric => !string.IsNullOrWhiteSpace(Lyric)
            && !string.Equals(Lyric.Trim(), "Not found", StringComparison.OrdinalIgnoreCase);
    }
}
