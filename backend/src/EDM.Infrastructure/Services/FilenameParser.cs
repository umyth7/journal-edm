using System.Text.RegularExpressions;
using EDM.Application.Interfaces.Infrastructure;
using EDM.Application.Models;

namespace EDM.Infrastructure.Services;

public class FilenameParser : IFilenameParser
{
    // Format: {YYYY}_{EventName}_{ArtistName}.ext
    // Ornek: 2026_Tomorrowland_r3hab.mp4
    // Ilk token: yil, son token(lar): sanatci, aradakiler: etkinlik
    private static readonly Regex Pattern =
        new(@"^(\d{4})_(.+)$", RegexOptions.Compiled);

    public FilenameParseResult Parse(string rawFileName)
    {
        if (string.IsNullOrWhiteSpace(rawFileName))
            return new FilenameParseResult(false, 0, string.Empty, string.Empty, "Dosya adi bos olamaz");

        // Uzantiyi kaldir
        var nameWithoutExt = Path.GetFileNameWithoutExtension(rawFileName);
        var match = Pattern.Match(nameWithoutExt);

        if (!match.Success)
            return new FilenameParseResult(false, 0, string.Empty, string.Empty,
                $"Desteklenmeyen format: '{rawFileName}'. Beklenen: YYYY_EventName_ArtistName.ext");

        if (!int.TryParse(match.Groups[1].Value, out var year) || year < 2000 || year > 2100)
            return new FilenameParseResult(false, 0, string.Empty, string.Empty,
                $"Gecersiz yil: '{match.Groups[1].Value}'");

        var tokens = match.Groups[2].Value.Split('_');

        // Son token sanatci, oncekiler etkinlik
        var artistRaw = tokens.Last();
        var eventTokens = tokens.Length > 1 ? tokens.Take(tokens.Length - 1).ToArray() : tokens;

        var artistName = ToTitleCase(artistRaw);
        var eventName = string.Join(" ", eventTokens.Select(ToTitleCase));

        return new FilenameParseResult(true, year, artistName, eventName);
    }

    private static string ToTitleCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpper(s[0]) + s.Substring(1).ToLower();
    }
}
