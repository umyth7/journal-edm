namespace EDM.Application.Interfaces.Infrastructure;

public interface IYouTubeService
{
    Task<string> UploadVideoAsync(string s3Key, string title, string description, CancellationToken cancellationToken);
}
