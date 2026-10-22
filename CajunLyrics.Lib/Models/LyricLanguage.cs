using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace CajunLyrics.Lib.Models;

/// <summary>
/// Represents a language filter option for CajunLyrics searches.
/// </summary>
public sealed record LyricLanguage : IParsable<LyricLanguage>
{
    public static readonly LyricLanguage French = new("fr", "French");
    public static readonly LyricLanguage LouisianaFrench = new("fr_LA", "Louisiana French");
    public static readonly LyricLanguage QuebecoisFrench = new("fr_QC", "Québécois French");
    public static readonly LyricLanguage AcadianFrench = new("fr_AC", "Acadian French");
    public static readonly LyricLanguage English = new("en", "English");

    /// <summary>
    /// Pseudo-language meaning "no language filter" — searches every upstream language.
    /// </summary>
    public static readonly LyricLanguage Any = new("all", "Any language");

    public static readonly IReadOnlyList<LyricLanguage> All = [French, LouisianaFrench, QuebecoisFrench, AcadianFrench, English, Any];

    /// <summary>
    /// The languages searched when a request does not specify any:
    /// Louisiana French, Acadian French, and Québécois French.
    /// </summary>
    public static readonly IReadOnlyList<LyricLanguage> DefaultSearch = [LouisianaFrench, AcadianFrench, QuebecoisFrench];

    public string Code { get; }
    public string DisplayName { get; }

    [JsonConstructor]
    public LyricLanguage(string code, string? displayName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
        DisplayName = string.IsNullOrWhiteSpace(displayName)
            ? All.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase))?.DisplayName ?? code
            : displayName;
    }

    public static LyricLanguage FromCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return All.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(l.DisplayName, code, StringComparison.OrdinalIgnoreCase))
               ?? new LyricLanguage(code);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out LyricLanguage result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = null;
            return false;
        }

        result = FromCode(s);
        return true;
    }

    public static LyricLanguage Parse(string s, IFormatProvider? provider = null)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException($"String '{s}' was not recognized as a valid {nameof(LyricLanguage)}.");
        }
        return result;
    }

    public override string ToString() => Code;

    public static implicit operator string?(LyricLanguage? language) => language?.Code;
    public static implicit operator LyricLanguage?(string? code) => string.IsNullOrWhiteSpace(code) ? null : FromCode(code);
}
