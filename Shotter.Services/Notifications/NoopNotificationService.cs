using Shotter.Core.Interfaces;

namespace Shotter.Services.Notifications;

public class NoopNotificationService : INotificationService
{
    public Task SendNotification(string message, CancellationToken cancellationToken) => Task.CompletedTask;
}