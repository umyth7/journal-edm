namespace EDM.Application.Interfaces.Infrastructure;

public interface IInstagramService
{
    Task<string> PublishReelAsync(string s3Key, string caption, CancellationToken cancellationToken);
}
