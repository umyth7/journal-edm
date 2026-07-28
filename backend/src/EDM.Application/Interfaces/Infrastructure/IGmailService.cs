using EDM.Application.Models;

namespace EDM.Application.Interfaces.Infrastructure;

public interface IGmailService
{
    Task<List<GmailMessage>> GetNewMessagesAsync(CancellationToken cancellationToken);
    Task MarkAsReadAsync(string messageId, CancellationToken cancellationToken);
}
