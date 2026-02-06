using System.Xml.Serialization;

namespace CajunLyrics.Lib
{
    [XmlRoot("GetLyricResult", Namespace = "http://api.cajunlyrics.com/")]
    public class LyricResult
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("LyricsUrl")]
        public required string LyricsUrl { get; set; }

        [XmlElement("ArtistUrl")]
        public required string ArtistUrl { get; set; }

        [XmlElement("Artist")]
        public required string Artist { get; set; }

        [XmlElement("Title")]
        public required string Title { get; set; }

        [XmlElement("Lyric")]
        public required string Lyric { get; set; }
    }
}
