namespace EDM.Application.Models;

public record GmailMessage(
    string MessageId,
    string Subject,
    string Body,
    string? DriveLink,
    string? FileName,
    DateTime ReceivedAt
);
