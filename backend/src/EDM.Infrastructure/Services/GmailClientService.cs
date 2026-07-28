using System.Text.RegularExpressions;
using EDM.Application.Interfaces.Infrastructure;
using EDM.Application.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EDM.Infrastructure.Services;

public class GmailClientService : IGmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GmailClientService> _logger;
    private static readonly Regex DriveLinkRegex = new(@"https://drive\.google\.com/[^\s""<>]+", RegexOptions.Compiled);
    private static readonly Regex FileNameRegex = new(@"\b[\w\-]+\.(mp4|mov|avi|mkv)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public GmailClientService(IConfiguration configuration, ILogger<GmailClientService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private GmailService CreateGmailApiService()
    {
        var clientId = _configuration["Gmail:ClientId"] ?? throw new InvalidOperationException("Gmail:ClientId is not configured");
        var clientSecret = _configuration["Gmail:ClientSecret"] ?? throw new InvalidOperationException("Gmail:ClientSecret is not configured");
        var refreshToken = _configuration["Gmail:RefreshToken"] ?? throw new InvalidOperationException("Gmail:RefreshToken is not configured");

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
            Scopes = new[] { GmailService.Scope.GmailReadonly, GmailService.Scope.GmailModify }
        });

        var token = new TokenResponse { RefreshToken = refreshToken };
        var credential = new UserCredential(flow, "user", token);

        return new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "EDM Journal"
        });
    }

    public async Task<List<GmailMessage>> GetNewMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var userId = _configuration["Gmail:UserId"] ?? "me";
            var service = CreateGmailApiService();

            var listRequest = service.Users.Messages.List(userId);
            listRequest.Q = "is:unread";
            listRequest.MaxResults = 10;

            var listResponse = await listRequest.ExecuteAsync(cancellationToken);
            var messages = new List<GmailMessage>();

            if (listResponse.Messages == null) return messages;

            foreach (var msg in listResponse.Messages)
            {
                var getRequest = service.Users.Messages.Get(userId, msg.Id);
                getRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
                var fullMsg = await getRequest.ExecuteAsync(cancellationToken);

                var subject = fullMsg.Payload?.Headers?.FirstOrDefault(h => h.Name == "Subject")?.Value ?? "";
                var dateStr = fullMsg.Payload?.Headers?.FirstOrDefault(h => h.Name == "Date")?.Value;
                var receivedAt = dateStr != null ? DateTime.Parse(dateStr) : DateTime.UtcNow;

                var body = ExtractBody(fullMsg);
                var driveLink = DriveLinkRegex.Match(body).Value;
                var fileName = FileNameRegex.Match(subject + " " + body).Value;

                messages.Add(new GmailMessage(
                    msg.Id,
                    subject,
                    body,
                    string.IsNullOrEmpty(driveLink) ? null : driveLink,
                    string.IsNullOrEmpty(fileName) ? null : fileName,
                    receivedAt
                ));
            }

            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gmail mesajları alınırken hata oluştu");
            return new List<GmailMessage>();
        }
    }

    public async Task MarkAsReadAsync(string messageId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _configuration["Gmail:UserId"] ?? "me";
            var service = CreateGmailApiService();

            var modifyRequest = new ModifyMessageRequest
            {
                RemoveLabelIds = new List<string> { "UNREAD" }
            };

            await service.Users.Messages.Modify(modifyRequest, userId, messageId).ExecuteAsync(cancellationToken);
            _logger.LogInformation("Mesaj okundu olarak işaretlendi: {MessageId}", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mesaj okundu işaretlenirken hata: {MessageId}", messageId);
        }
    }

    private static string ExtractBody(Message message)
    {
        if (message.Payload?.Body?.Data != null)
            return DecodeBase64(message.Payload.Body.Data);

        if (message.Payload?.Parts != null)
        {
            foreach (var part in message.Payload.Parts)
            {
                if (part.MimeType == "text/plain" && part.Body?.Data != null)
                    return DecodeBase64(part.Body.Data);
            }
        }

        return string.Empty;
    }

    private static string DecodeBase64(string base64)
    {
        var bytes = Convert.FromBase64String(base64.Replace('-', '+').Replace('_', '/'));
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
