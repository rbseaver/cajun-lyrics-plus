using CajunLyrics.Lib.Models;
using CajunLyrics.Lib.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using CajunLyrics.Lib.Configuration;

namespace CajunLyrics.Tests.Integration
{
    [TestFixture]
    [Category("Lib")]
    public class WhenCallingCajunLyricsService
    {
        private ILyricsService? cajunLyricsService;

        [SetUp]
        public void Setup()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var services = new ServiceCollection();
            services.Configure<HttpClientOptions>(configuration.GetSection("HttpClient"));
            services.AddHttpClient(
                nameof(CajunLyricsService),
                (sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<HttpClientOptions>>().Value;
                    client.BaseAddress = options.BaseAddress;
                });
            services.AddScoped<ILyricsService, CajunLyricsService>();
            var provider = services.BuildServiceProvider();
            cajunLyricsService = provider.GetRequiredService<ILyricsService>();
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
                    }, opts => opts.IgnoringNewlineStyle());
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
