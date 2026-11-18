namespace CajunLyrics.Lib.Services;

public sealed class TranslationNotConfiguredException : InvalidOperationException
{
    public TranslationNotConfiguredException()
        : base("The DeepL API key is not configured.")
    {
    }
}
