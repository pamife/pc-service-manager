namespace PcServiceManager.Core.Interfaces;

public interface INotificationService
{
    void ShowToastNotification(string title, string message, string? actionArg = null);
    void ShowOverdueNotification(string pcName, int overdueCount, int dueSoonCount);
    void ShowServiceCompletedNotification(string pcName, string templateName, int completedCount);
}
