using AutoFixture;
using CajunLyrics.Api.Controllers;
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
        var language = fixture.Create<string>();

        var expectedLyricResult = fixture.Create<LyricResult>();
        cajunLyricsServiceMock.GetSongLyricsAsync(
            Arg.Is<LyricSearchRequest>(r => r.Artist == artist && r.Title == title && r.Language == language))
            .Returns(Task.FromResult(expectedLyricResult));


        var result = (OkObjectResult)await controller.Get(artist, title, language);

        result.Value.Should().BeEquivalentTo(expectedLyricResult);
    }

    [Test]
    public async Task ShouldReturn404WhenLyricIsNull()
    {
        var artist = fixture.Create<string>();
        var title = fixture.Create<string>();
        var language = fixture.Create<string>();

        var expectedLyricResult = fixture.Build<LyricResult>()
            .With(l => l.Lyric, string.Empty)
            .Create();
        cajunLyricsServiceMock.GetSongLyricsAsync(
            Arg.Is<LyricSearchRequest>(
                r => r.Artist == artist && r.Title == title && r.Language == language))
            .Returns(Task.FromResult(expectedLyricResult));

        var result = (NotFoundObjectResult)await controller.Get(artist, title, language);

        result.Value.Should().Be($"Lyrics not found for '{artist}' and '{title}");
    }

    [Test]
    public async Task ShouldHandleInternalErrorGracefully()
    {
        var artist = fixture.Create<string>();
        var title = fixture.Create<string>();
        var language = fixture.Create<string>();

        cajunLyricsServiceMock.GetSongLyricsAsync(Arg.Any<LyricSearchRequest>())
            .Throws<InvalidOperationException>();

        var result = (ObjectResult)await controller.Get(artist, title, language);

        result.StatusCode.Should().Be(500);
        result.Value.Should().Be("An error occurred while attempting to retrieve lyrics.");
    }
}
