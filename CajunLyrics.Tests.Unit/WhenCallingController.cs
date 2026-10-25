using AutoFixture;
using CajunLyrics.Api.Controllers;
using CajunLyrics.Lib;
using CajunLyrics.Lib.Models;
using CajunLyrics.Lib.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CajunLyrics.Tests.Unit;

[TestFixture]
[Category("Controllers")]
public class WhenCallingController
{
    private Fixture fixture;
    private ILyricsService cajunLyricsServiceMock;
    private LyricsController controller;

    [SetUp]
    public void Setup()
    {
        fixture = new Fixture();
        cajunLyricsServiceMock = Substitute.For<ILyricsService>();
        controller = new LyricsController(cajunLyricsServiceMock);
    }

    [Test]
    public async Task ShouldRespondWithDirectLyricResult()
    {
        var artist = fixture.Create<string>();
        var title = fixture.Create<string>();
        List<LyricLanguage> languages = [LyricLanguage.LouisianaFrench];

        var expectedLyricResult = fixture.Create<LyricResult>();
        cajunLyricsServiceMock.GetSongLyricsAsync(
            Arg.Is<LyricSearchRequest>(r => r.Artist == artist && r.Title == title && r.Languages == languages))
            .Returns(Task.FromResult(expectedLyricResult));


        var result = (OkObjectResult)await controller.Get(artist, title, languages);

        result.Value.Should().BeEquivalentTo(expectedLyricResult);
    }

    [Test]
    public async Task ShouldDelegateToServiceDefaultsWhenNoLanguageSpecified()
    {
        var artist = fixture.Create<string>();
        var title = fixture.Create<string>();

        var expectedLyricResult = fixture.Create<LyricResult>();
        cajunLyricsServiceMock.GetSongLyricsAsync(
            Arg.Is<LyricSearchRequest>(r => r.Artist == artist && r.Title == title && r.Languages == null))
            .Returns(Task.FromResult(expectedLyricResult));

        var result = (OkObjectResult)await controller.Get(artist, title, null);

        result.Value.Should().BeEquivalentTo(expectedLyricResult);
    }

    [Test]
    public async Task ShouldPassMultipleSearchLanguagesThroughToService()
    {
        var artist = fixture.Create<string>();
        List<LyricLanguage> languages = [LyricLanguage.English, LyricLanguage.French];

        var expectedResult = fixture.Create<LyricSearchResult>();
        cajunLyricsServiceMock.GetSearchResultsAsync(
            Arg.Is<LyricSearchRequest>(r => r.Artist == artist && r.Languages == languages))
            .Returns(Task.FromResult(expectedResult));

        var result = (OkObjectResult)await controller.Search(artist, null, languages);

        result.Value.Should().BeEquivalentTo(expectedResult);
    }

    [TestCase(null, null)]
    [TestCase("", " ")]
    [TestCase(" ", null)]
    public async Task ShouldRejectSearchWithoutAnArtistOrTitle(string? artist, string? title)
    {
        var result = await controller.Search(artist, title, null);

        result.Should().BeOfType<BadRequestObjectResult>();
        await cajunLyricsServiceMock.DidNotReceive()
            .GetSearchResultsAsync(Arg.Any<LyricSearchRequest>());
    }

    [Test]
    public async Task ShouldRejectSearchWithUnsupportedLanguage()
    {
        var result = await controller.Search("artist", null, [LyricLanguage.FromCode("unsupported")]);

        result.Should().BeOfType<BadRequestObjectResult>();
        await cajunLyricsServiceMock.DidNotReceive()
            .GetSearchResultsAsync(Arg.Any<LyricSearchRequest>());
    }

    [Test]
    public async Task ShouldRejectSearchWithTooManyLanguages()
    {
        var languages = Enumerable.Repeat(LyricLanguage.English, LyricLanguage.All.Count + 1).ToList();

        var result = await controller.Search("artist", null, languages);

        result.Should().BeOfType<BadRequestObjectResult>();
        await cajunLyricsServiceMock.DidNotReceive()
            .GetSearchResultsAsync(Arg.Any<LyricSearchRequest>());
    }

    [TestCase(" ", "title", null)]
    [TestCase("artist", " ", null)]
    [TestCase("artist", "title", 0)]
    [TestCase("artist", "title", -1)]
    public async Task ShouldRejectLyricRequestsWithInvalidRequiredValues(string? artist, string? title, int? id)
    {
        var result = await controller.Get(artist, title, null, id);

        result.Should().BeOfType<BadRequestObjectResult>();
        await cajunLyricsServiceMock.DidNotReceive()
            .GetSongLyricsAsync(Arg.Any<LyricSearchRequest>());
        await cajunLyricsServiceMock.DidNotReceive()
            .GetSongLyricsByIdAsync(Arg.Any<int>(), Arg.Any<IReadOnlyList<LyricLanguage>?>());
    }

    [Test]
    public async Task ShouldRejectLyricRequestWithUnsupportedLanguage()
    {
        var result = await controller.Get("artist", "title", [LyricLanguage.FromCode("unsupported")]);

        result.Should().BeOfType<BadRequestObjectResult>();
        await cajunLyricsServiceMock.DidNotReceive()
            .GetSongLyricsAsync(Arg.Any<LyricSearchRequest>());
    }

    [Test]
    public async Task ShouldReturn404WhenLyricIsNull()
    {
        var artist = fixture.Create<string>();
        var title = fixture.Create<string>();
        List<LyricLanguage> languages = [LyricLanguage.French];

        var expectedLyricResult = fixture.Build<LyricResult>()
            .With(l => l.Lyric, string.Empty)
            .Create();
        cajunLyricsServiceMock.GetSongLyricsAsync(
            Arg.Is<LyricSearchRequest>(
                r => r.Artist == artist && r.Title == title && r.Languages == languages))
            .Returns(Task.FromResult(expectedLyricResult));

        var result = (NotFoundObjectResult)await controller.Get(artist, title, languages);

        result.Value.Should().Be($"Lyrics not found for '{artist}' and '{title}'");
    }

    [Test]
    public async Task ShouldHandleInternalErrorGracefully()
    {
        var artist = fixture.Create<string>();
        var title = fixture.Create<string>();
        List<LyricLanguage> languages = [LyricLanguage.QuebecoisFrench];

        cajunLyricsServiceMock.GetSongLyricsAsync(Arg.Any<LyricSearchRequest>())
            .Throws<InvalidOperationException>();

        var result = (ObjectResult)await controller.Get(artist, title, languages);

        result.StatusCode.Should().Be(500);
        result.Value.Should().Be("An error occurred while attempting to retrieve lyrics.");
    }
}
