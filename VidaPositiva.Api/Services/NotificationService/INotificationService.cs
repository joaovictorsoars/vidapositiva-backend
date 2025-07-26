namespace VidaPositiva.Api.Services.NotificationService;

public interface INotificationService
{
    Task NotifyProgressAsync(string connectionId, object obj);
}