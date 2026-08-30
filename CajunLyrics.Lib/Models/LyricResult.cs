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
    }
}
