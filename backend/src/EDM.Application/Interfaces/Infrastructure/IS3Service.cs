namespace EDM.Application.Interfaces.Infrastructure;

public interface IS3Service
{
    Task<string> UploadAsync(Stream fileStream, string s3Key, CancellationToken cancellationToken);
    string GetPresignedUrl(string s3Key, TimeSpan expiry);
}
