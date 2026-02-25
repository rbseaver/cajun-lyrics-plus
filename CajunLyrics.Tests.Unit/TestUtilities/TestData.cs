using AutoFixture;
using static System.Net.WebRequestMethods;

namespace CajunLyrics.Tests.Unit.TestUtilities
{
    internal static class TestData
    {
        public static int[] Id = [2891, 2892];
        public static string[] LyricsUrl =
        [
            "http://www.cajunlyrics.com/?lyrics=2891",
            "http://www.cajunlyrics.com/?lyrics=2892"
        ];
        public static string[] ArtistUrl =
        [
            "http://www.cajunlyrics.com/?page=search&amp;artist=168",
            "http://www.cajunlyrics.com/?page=search&amp;artist=169"
        ];
        public static string[] Artist = { "Dewey Balfa", "Canray Fontenot" };
        public static string[] Title = { "J’ai Pleuré", "Malinda" };
        public static string Lyric = "Lyrics go here";

        internal static string ExpectedLyricResult = @$"<?xml version='1.0' encoding='UTF-8'?>
<GetLyricResult xmlns='http://api.cajunlyrics.com/'>
    <Id>{Id[0]}</Id>
    <LyricsUrl>{LyricsUrl[0]}</LyricsUrl>
    <ArtistUrl>{ArtistUrl[0]}</ArtistUrl>
    <Artist>{Artist[0]}</Artist>
    <Title>{Title[0]}</Title>
    <Lyric>{Lyric}</Lyric>
</GetLyricResult>
";
        internal static string ExpectedSearchResult = @$"<?xml version='1.0' encoding='UTF-8'?>
<SearchLyricResultArray xmlns='http://api.cajunlyrics.com/'>
  <SearchLyricsResult>
    <Id>{Id[0]}</Id>
    <LyricsUrl>{LyricsUrl[0]}</LyricsUrl>
    <ArtistUrl>{ArtistUrl[0]}</ArtistUrl>
    <Artist>{Artist[0]}</Artist>
    <Title>{Title[0]}</Title>
  </SearchLyricsResult>
  <SearchLyricsResult>
    <Id>{Id[1]}</Id>
    <LyricsUrl>{LyricsUrl[1]}</LyricsUrl>
    <ArtistUrl>{ArtistUrl[1]}</ArtistUrl>
    <Artist>{Artist[1]}</Artist>
    <Title>{Title[1]}</Title>
  </SearchLyricsResult>
</SearchLyricResultArray>";
    }
}