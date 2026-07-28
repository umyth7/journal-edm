using System.Text.RegularExpressions;
using EDM.Application.Interfaces.Infrastructure;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EDM.Infrastructure.Services;

public class GoogleDriveService : IGoogleDriveService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleDriveService> _logger;
    private static readonly Regex FileIdRegex = new(@"(?:/d/|id=)([a-zA-Z0-9_-]+)", RegexOptions.Compiled);

    public GoogleDriveService(IConfiguration configuration, ILogger<GoogleDriveService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string ParseFileId(string driveLink)
    {
        var match = FileIdRegex.Match(driveLink);
        if (!match.Success)
            throw new ArgumentException($"Drive linkinden fileId çıkarılamadı: {driveLink}");
        return match.Groups[1].Value;
    }

    public async Task<Stream> DownloadFileAsync(string driveLink, CancellationToken cancellationToken)
    {
        var fileId = ParseFileId(driveLink);

        var clientId = _configuration["GoogleDrive:ClientId"] ?? throw new InvalidOperationException("GoogleDrive:ClientId is not configured");
        var clientSecret = _configuration["GoogleDrive:ClientSecret"] ?? throw new InvalidOperationException("GoogleDrive:ClientSecret is not configured");
        var refreshToken = _configuration["GoogleDrive:RefreshToken"] ?? throw new InvalidOperationException("GoogleDrive:RefreshToken is not configured");

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
            Scopes = new[] { DriveService.Scope.DriveReadonly }
        });

        var token = new TokenResponse { RefreshToken = refreshToken };
        var credential = new UserCredential(flow, "user", token);

        var driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "EDM Journal"
        });

        var request = driveService.Files.Get(fileId);
        var memoryStream = new MemoryStream();
        await request.DownloadAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        _logger.LogInformation("Drive dosyası indirildi: {FileId}, Boyut: {Size} bytes", fileId, memoryStream.Length);
        return memoryStream;
    }
}
