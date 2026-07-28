namespace EDM.Application.Interfaces.Infrastructure;

public interface IContentPipelineService
{
    Task ExecuteAsync(Guid contentJobId, CancellationToken cancellationToken);
}
