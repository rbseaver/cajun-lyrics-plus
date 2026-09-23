using CajunLyrics.Lib.Models;
using FluentAssertions;

namespace CajunLyrics.Tests.Unit;

[TestFixture]
[Category("Lib")]
public class WhenUsingLyricLanguage
{
    [Test]
    public void ShouldHaveExpectedPredefinedLanguages()
    {
        LyricLanguage.French.Code.Should().Be("fr");
        LyricLanguage.French.DisplayName.Should().Be("French");

        LyricLanguage.LouisianaFrench.Code.Should().Be("fr_LA");
        LyricLanguage.LouisianaFrench.DisplayName.Should().Be("Louisiana French");

        LyricLanguage.QuebecoisFrench.Code.Should().Be("fr_QC");
        LyricLanguage.QuebecoisFrench.DisplayName.Should().Be("Québécois French");

        LyricLanguage.AcadianFrench.Code.Should().Be("fr_AC");
        LyricLanguage.AcadianFrench.DisplayName.Should().Be("Acadian French");

        LyricLanguage.English.Code.Should().Be("en");
        LyricLanguage.English.DisplayName.Should().Be("English");

        LyricLanguage.All.Should().HaveCount(5)
            .And.Contain([
                LyricLanguage.French,
                LyricLanguage.LouisianaFrench,
                LyricLanguage.QuebecoisFrench,
                LyricLanguage.AcadianFrench,
                LyricLanguage.English
            ]);
    }

    [TestCase("fr", "fr", "French")]
    [TestCase("FR", "fr", "French")]
    [TestCase("fr_LA", "fr_LA", "Louisiana French")]
    [TestCase("FR_la", "fr_LA", "Louisiana French")]
    [TestCase("fr_QC", "fr_QC", "Québécois French")]
    [TestCase("FR_qc", "fr_QC", "Québécois French")]
    [TestCase("fr_AC", "fr_AC", "Acadian French")]
    [TestCase("FR_ac", "fr_AC", "Acadian French")]
    [TestCase("en", "en", "English")]
    [TestCase("Louisiana French", "fr_LA", "Louisiana French")]
    [TestCase("louisiana french", "fr_LA", "Louisiana French")]
    [TestCase("Acadian French", "fr_AC", "Acadian French")]
    [TestCase("acadian french", "fr_AC", "Acadian French")]
    [TestCase("Québécois French", "fr_QC", "Québécois French")]
    [TestCase("québécois french", "fr_QC", "Québécois French")]
    public void ShouldResolveKnownLanguagesFromCodeOrDisplayName(string input, string expectedCode, string expectedDisplayName)
    {
        var language = LyricLanguage.FromCode(input);

        language.Code.Should().Be(expectedCode);
        language.DisplayName.Should().Be(expectedDisplayName);
    }

    [Test]
    public void ShouldSupportCustomLanguageCode()
    {
        var custom = LyricLanguage.FromCode("es");

        custom.Code.Should().Be("es");
        custom.DisplayName.Should().Be("es");
    }

    [Test]
    public void ShouldThrowWhenFromCodeIsNullOrEmpty()
    {
        var actNull = () => LyricLanguage.FromCode(null!);
        var actEmpty = () => LyricLanguage.FromCode("   ");

        actNull.Should().Throw<ArgumentException>();
        actEmpty.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ShouldSupportTryParse()
    {
        var success = LyricLanguage.TryParse("fr_LA", null, out var result);

        success.Should().BeTrue();
        result.Should().Be(LyricLanguage.LouisianaFrench);

        LyricLanguage.TryParse(null, null, out var nullResult).Should().BeFalse();
        nullResult.Should().BeNull();

        LyricLanguage.TryParse("   ", null, out var emptyResult).Should().BeFalse();
        emptyResult.Should().BeNull();
    }

    [Test]
    public void ShouldSupportParse()
    {
        var result = LyricLanguage.Parse("fr_QC");
        result.Should().Be(LyricLanguage.QuebecoisFrench);

        var act = () => LyricLanguage.Parse(string.Empty);
        act.Should().Throw<FormatException>();
    }

    [Test]
    public void ShouldConvertImplicitlyToAndFromString()
    {
        string? code = LyricLanguage.LouisianaFrench;
        code.Should().Be("fr_LA");

        LyricLanguage? language = "fr_LA";
        language.Should().Be(LyricLanguage.LouisianaFrench);

        LyricLanguage? nullLanguage = (string?)null;
        nullLanguage.Should().BeNull();
    }

    [Test]
    public void ShouldReturnCodeOnToString()
    {
        LyricLanguage.LouisianaFrench.ToString().Should().Be("fr_LA");
        LyricLanguage.QuebecoisFrench.ToString().Should().Be("fr_QC");
    }
}
