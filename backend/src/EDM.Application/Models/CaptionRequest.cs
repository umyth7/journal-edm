using EDM.Domain.Enums;

namespace EDM.Application.Models;

public record CaptionRequest(
    string ArtistName,
    string EventName,
    int Year,
    Platform Platform
);
