using AutoFixture;
using CajunLyrics.Api.Controllers;
using CajunLyrics.Lib.Models;
using CajunLyrics.Lib.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CajunLyrics.Tests.Unit;

[TestFixture]
[Category("Controllers")]
public class WhenCallingController
{
    private Fixture fixture;

    [SetUp]
    public void Setup() { fixture = new Fixture(); }

    [Test]
    public async Task ShouldRespondWithDirectLyricResult()
    {
        var artist = fixture.Create<string>();
        var title = fixture.Create<string>();
        var language = fixture.Create<string>();

        var expectedLyricResult = fixture.Create<LyricResult>();
        var cajunLyricsServiceMock = Substitute.For<ILyricsService>();
        cajunLyricsServiceMock.GetSongLyricsAsync(
            Arg.Is<LyricSearchRequest>(r => r.Artist == artist && r.Title == title && r.Language == language))
            .Returns(Task.FromResult(expectedLyricResult));
        
        var controller = new LyricsController(cajunLyricsServiceMock);

        var result = (OkObjectResult)await controller.Get(artist, title, language);

        result.Value.Should().BeEquivalentTo(expectedLyricResult);
    }
}
