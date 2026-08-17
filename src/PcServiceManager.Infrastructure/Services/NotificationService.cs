using System.Diagnostics;
using PcServiceManager.Core.Interfaces;

namespace PcServiceManager.Infrastructure.Services;

public class NotificationService : INotificationService
{
    public void ShowToastNotification(string title, string message, string? actionArg = null)
    {
        try
        {
            if (!OperatingSystem.IsWindows()) return;

            // Use PowerShell to trigger a modern native Windows Toast notification
            // Escape double quotes and special characters
            var safeTitle = title.Replace("\"", "`\"").Replace("'", "`'");
            var safeMessage = message.Replace("\"", "`\"").Replace("'", "`'");

            var psScript = $@"
[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
[Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] | Out-Null
$template = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent([Windows.UI.Notifications.ToastTemplateType]::ToastText02)
$nodes = $template.GetElementsByTagName('text')
$nodes.Item(0).AppendChild($template.CreateTextNode('{safeTitle}')) | Out-Null
$nodes.Item(1).AppendChild($template.CreateTextNode('{safeMessage}')) | Out-Null
$toast = [Windows.UI.Notifications.ToastNotification]::new($template)
$notifier = [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('PC Service Manager')
$notifier.Show($toast)
";

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript.Replace("\r\n", " ").Replace("\n", " ")}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(startInfo);
        }
        catch
        {
            // Fail silently if notifications are restricted by policy
        }
    }

    public void ShowOverdueNotification(string pcName, int overdueCount, int dueSoonCount)
    {
        if (overdueCount > 0)
        {
            ShowToastNotification(
                "PC Service Manager - Maintenance Overdue",
                $"{pcName} has {overdueCount} overdue maintenance task{(overdueCount == 1 ? "" : "s")}. Please perform a service session soon.");
        }
        else if (dueSoonCount > 0)
        {
            ShowToastNotification(
                "PC Service Manager - Maintenance Due Soon",
                $"{pcName} has {dueSoonCount} maintenance task{(dueSoonCount == 1 ? "" : "s")} due soon.");
        }
    }

    public void ShowServiceCompletedNotification(string pcName, string templateName, int completedCount)
    {
        ShowToastNotification(
            "Service Session Saved",
            $"Completed {completedCount} tasks for {pcName} ({templateName}). Digital service history updated.");
    }
}
