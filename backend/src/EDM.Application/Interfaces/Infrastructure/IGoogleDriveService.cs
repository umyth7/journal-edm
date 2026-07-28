namespace EDM.Application.Interfaces.Infrastructure;

public interface IGoogleDriveService
{
    Task<Stream> DownloadFileAsync(string driveLink, CancellationToken cancellationToken);
    string ParseFileId(string driveLink);
}
