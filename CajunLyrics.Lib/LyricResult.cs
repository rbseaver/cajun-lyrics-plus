using System.Xml.Serialization;

namespace CajunLyrics.Lib
{
    [XmlRoot("GetLyricResult", Namespace = "http://api.cajunlyrics.com/")]
    public class LyricResult
    {
        [XmlElement("Id")]
        public int Id { get; set; }

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
    }
}
