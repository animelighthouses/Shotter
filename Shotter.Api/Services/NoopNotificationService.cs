namespace Shotter.Services;

public class NoopNotificationService : INotificationService
{
    public Task SendNotification(string message, CancellationToken cancellationToken) => Task.CompletedTask;
}