using CajunLyrics.Api.Controllers;
using CajunLyrics.Lib.Models;
using CajunLyrics.Lib.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CajunLyrics.Tests.Unit;

[TestFixture]
[Category("Controllers")]
public class WhenCallingTranslationController
{
    private ITranslationService translationService = null!;
    private TranslationController controller = null!;

    [SetUp]
    public void Setup()
    {
        translationService = Substitute.For<ITranslationService>();
        controller = new TranslationController(
            translationService,
            Substitute.For<ILogger<TranslationController>>());
    }

    [Test]
    public async Task ShouldTranslateTextToEnglish()
    {
        translationService.TranslateToEnglishAsync("Bonjour, mon ami")
            .Returns("Hello, my friend");

        var result = (OkObjectResult)await controller.Translate(new TranslationRequest("Bonjour, mon ami"));

        result.Value.Should().BeEquivalentTo(new TranslationResponse("Hello, my friend"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task ShouldRejectEmptyText(string? text)
    {
        var result = await controller.Translate(new TranslationRequest(text));

        result.Should().BeOfType<BadRequestObjectResult>();
        await translationService.DidNotReceive().TranslateToEnglishAsync(Arg.Any<string>());
    }

    [Test]
    public async Task ShouldRejectTextLongerThanTheSupportedLimit()
    {
        var result = await controller.Translate(new TranslationRequest(new string('a', 5001)));

        result.Should().BeOfType<BadRequestObjectResult>();
        await translationService.DidNotReceive().TranslateToEnglishAsync(Arg.Any<string>());
    }

    [Test]
    public async Task ShouldReturnServiceUnavailableWhenDeepLIsNotConfigured()
    {
        translationService.TranslateToEnglishAsync(Arg.Any<string>())
            .Throws(new TranslationNotConfiguredException());

        var result = (ObjectResult)await controller.Translate(new TranslationRequest("Bonjour"));

        result.StatusCode.Should().Be(503);
    }

    [Test]
    public async Task ShouldReturnBadGatewayWhenDeepLTimesOut()
    {
        translationService.TranslateToEnglishAsync(Arg.Any<string>())
            .Throws(new TaskCanceledException());

        var result = (ObjectResult)await controller.Translate(new TranslationRequest("Bonjour"));

        result.StatusCode.Should().Be(502);
    }
}
