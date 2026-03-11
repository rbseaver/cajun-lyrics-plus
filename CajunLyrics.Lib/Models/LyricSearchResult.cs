using CajunLyrics.Lib.Models;
using System.Xml.Serialization;

namespace CajunLyrics.Lib
{
    [XmlRoot("SearchLyricResultArray", Namespace = "http://api.cajunlyrics.com/")]

    public class LyricSearchResult
    {
        [XmlElement("SearchLyricsResult")]
        public required List<LyricResult> LyricResults { get; set; }
    }
}