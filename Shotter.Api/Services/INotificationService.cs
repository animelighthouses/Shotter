namespace Shotter.Services;

public interface INotificationService
{
    Task SendNotification(string message, CancellationToken cancellationToken);
}