namespace EDM.Application.Interfaces.Infrastructure;

public interface ITikTokService
{
    Task<string> PublishVideoAsync(string s3Key, string caption, CancellationToken cancellationToken);
}
