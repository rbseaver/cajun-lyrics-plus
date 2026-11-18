namespace CajunLyrics.Lib.Services;

public interface ITranslationService
{
    Task<string> TranslateToEnglishAsync(string text, CancellationToken cancellationToken = default);
}
