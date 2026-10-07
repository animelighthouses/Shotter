namespace Shotter.Core.Interfaces;

public interface INotificationService
{
    Task SendNotification(string message, CancellationToken cancellationToken);
}