
# Developer API

The Developer API give you access to make searches and retrieve lyrics through an HTTP GET Protocol.

It allows you to search for lyrics by artist name and/or song title. All the results are in an XML file.

## LyricDirectSearch

https://api.cajunlyrics.com/LyricDirectSearch.php?keywords=[ARTIST - SONG TITLE]
https://api.cajunlyrics.com/LyricDirectSearch.php?artist=[ARTIST]&title=[SONG TITLE]
https://api.cajunlyrics.com/LyricDirectSearch.php?id=[ID]

The Output will look like this:


<GetLyricResult xmlns="https://api.cajunlyrics.com/">
  <Id> Lyrics ID </Id>
  <LyricsUrl> Lyrics Page URL </LyricsUrl>
  <ArtistUrl> Artist Page URL </ArtistUrl>
  <Artist> Artist Name </Artist>
  <Title> Song Title </Title>
  <Lyric> Lyrics </Lyric>
</GetLyricResult>


## LyricSearchList
https://api.cajunlyrics.com/LyricSearchList.php?keywords=[ARTIST]
https://api.cajunlyrics.com/LyricSearchList.php?keywords=[SONG TITLE]
https://api.cajunlyrics.com/LyricSearchList.php?keywords=[ARTIST - SONG TITLE]
https://api.cajunlyrics.com/LyricSearchList.php?artist=[ARTIST]&title=[SONG TITLE]

By default you will get a limit of 25 results.
You can change it to whatever you want by adding the parameter &limit=[NUMBER] to the URL.

The Output will look like this:

```xml
<SearchLyricResultArray xmlns="https://api.cajunlyrics.com/">
  <SearchLyricsResult>
    <Id> ID (RESULT #1) </Id>
    <LyricsUrl> Lyrics Page URL</LyricsUrl>
    <ArtistUrl> Artist Page URL </ArtistUrl>
    <Artist> Artist Name </Artist>
    <Title> Song Title </Title>
  </SearchLyricsResult>
  <SearchLyricsResult>
    <Id> ID (RESULT #2) </Id>
    <LyricsUrl> Lyrics Page URL</LyricsUrl>
    <ArtistUrl> Artist Page URL </ArtistUrl>
    <Artist> Artist Name </Artist>
    <Title> Song Title </Title>
  </SearchLyricsResult>
<SearchLyricResultArray>
```

## Filtering by Language
You can limit the search by language, by adding the parameter lf=[LANGUAGE CODE] or lf=[LANGUAGE CODE_REGION CODE]

For example:
https://api.cajunlyrics.com/LyricSearchList.php?keywords=[ARTIST]&lf=fr
https://api.cajunlyrics.com/LyricSearchList.php?keywords=[ARTIST]&lf=fr_LA

*Please note that we use a few non-standard language codes like, fr_LA (Louisiana French) or fr_QC (Québécois French)
If you filter using, for example, fr_CA (Canadian French) you will not get results that are tagged as fr_QC (Québécois French)*