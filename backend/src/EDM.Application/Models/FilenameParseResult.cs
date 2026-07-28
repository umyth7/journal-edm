namespace EDM.Application.Models;

public record FilenameParseResult(
    bool Success,
    int Year,
    string ArtistName,
    string EventName,
    string? ErrorMessage = null
);
