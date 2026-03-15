using System;

namespace CajunLyrics.Lib.Models
{
    public class LyricSearchRequest
    {
        public string? Artist { get; set; }
        public string? Title { get; set; }
        public string? Language { get; set; }
    }
}
