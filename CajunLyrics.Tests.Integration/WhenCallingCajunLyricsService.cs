using CajunLyrics.Lib.Models;
using CajunLyrics.Lib.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CajunLyrics.Tests.Integration
{
    [TestFixture]
    [Category("Lib")]
    public class WhenCallingCajunLyricsService
    {
        private IServiceScope scope = null!;
        private ILyricsService cajunLyricsService = null!;

        [SetUp]
        public void Setup()
        {
            // Same wiring the API uses: Program.cs binds HttpClient:BaseAddress
            // to the local stub, so there is no upstream, no rate limit and no
            // pacing between tests.
            scope = LocalUpstream.Factory.Services.CreateScope();
            cajunLyricsService = scope.ServiceProvider.GetRequiredService<ILyricsService>();
        }

        [TearDown]
        public void TearDown()
        {
            scope.Dispose();
        }

        [Test]
        public async Task ShouldRetrieveLyrics()
        {
            var result = await cajunLyricsService.GetSongLyricsAsync(
                new LyricSearchRequest { Artist = "feufollet", Title = "Blues de Dix Ans" });

            result.Should().NotBeNull();
            result.Should()
                .BeEquivalentTo(
                    new LyricResult
                    {
                        Artist = "Feufollet",
                        Title = "Blues De Dix Ans",
                        LyricsUrl = "http://www.cajunlyrics.com/?lyrics=512",
                        ArtistUrl = "http://www.cajunlyrics.com/?page=search&artist=16",
                        Id = 512,
                        Lyric = @"Dix ans, dix ans, c’est l’ans pour espérer
Avec une pierre tout le temps pour mon oreiller
Dix ans, dix ans, c’est l’ans pour misérer
Mais pour quelque chose que moi j’avais pas fait

C’est toi qui m’a accuse, c’est lui qui m’a condamne
Et lui il était content de m’envoyer
Ils m’ont mal traite, quand même je les ai demande
De me pardonner pour ca j’avais pas fait

Quand ils m’ont pardonne, après ce grand donne
Quand tu m’a vu tu t’es mis à pleurer
Toi t’as réalisé le mal tu m’avais fais
Et comment gros que j’avais misérer

Lyrics Provided by CajunLyrics.com"
                    }, opts => opts.IgnoringNewlineStyle().Excluding(l => l.Language));

            // Provenance, not upstream payload: whichever default language matched.
            result.Language.Should().NotBeNull();
            LyricLanguage.DefaultSearch.Should().Contain(result.Language);
        }

        [Test]
        public async Task ShouldRetrieveSearchResults()
        {
            var result = await cajunLyricsService.GetSearchResultsAsync(
                new LyricSearchRequest { Artist = "feufollet", Title = "Blues de Dix Ans" });

            result.LyricResults.Should().NotBeNull();
            result.LyricResults.Count.Should().BeGreaterThan(0);
            result.LyricResults.Should()
                .ContainEquivalentOf(
                    new LyricResult
                    {
                        Artist = "Feufollet",
                        Title = "Blues De Dix Ans",
                        LyricsUrl = "http://www.cajunlyrics.com/?lyrics=512",
                        ArtistUrl = "http://www.cajunlyrics.com/?page=search&artist=16",
                        Id = 512,
                    });
        }
    }
}
