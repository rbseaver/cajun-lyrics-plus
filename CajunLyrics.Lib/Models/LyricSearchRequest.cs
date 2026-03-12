using System;

namespace CajunLyrics.Lib.Models
{
    public class LyricSearchRequest
    {
        public required string Artist { get; set; }
        public required string Title { get; set; }
        public string? Language { get; set; }
    }
}
