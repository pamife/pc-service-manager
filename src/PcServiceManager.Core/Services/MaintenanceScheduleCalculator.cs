using PcServiceManager.Core.Enums;

namespace PcServiceManager.Core.Services;

public static class MaintenanceScheduleCalculator
{
    public static DateTime? CalculateNextDueDate(DateTime baseDate, IntervalType intervalType, int intervalValue)
    {
        if (intervalValue <= 0 && intervalType != IntervalType.Disabled && intervalType != IntervalType.OneTime)
        {
            intervalValue = 1;
        }

        return intervalType switch
        {
            IntervalType.Days => baseDate.AddDays(intervalValue),
            IntervalType.Weeks => baseDate.AddDays(intervalValue * 7),
            IntervalType.Months => baseDate.AddMonths(intervalValue),
            IntervalType.Years => baseDate.AddYears(intervalValue),
            IntervalType.Custom => baseDate.AddDays(intervalValue),
            IntervalType.OneTime => null,
            IntervalType.Disabled => null,
            _ => baseDate.AddMonths(1)
        };
    }

    public static MaintenanceStatus CalculateTaskStatus(
        DateTime? nextDueDate,
        DateTime? lastPerformedDate,
        IntervalType intervalType,
        bool isEnabled,
        int dueSoonDaysThreshold,
        DateTime referenceDate)
    {
        if (!isEnabled || intervalType == IntervalType.Disabled)
        {
            return MaintenanceStatus.Disabled;
        }

        if (intervalType == IntervalType.OneTime && lastPerformedDate.HasValue)
        {
            return MaintenanceStatus.Good;
        }

        if (!nextDueDate.HasValue)
        {
            // Never performed or no due date configured
            return lastPerformedDate.HasValue ? MaintenanceStatus.Good : MaintenanceStatus.Overdue;
        }

        var dueDate = nextDueDate.Value.Date;
        var today = referenceDate.Date;

        if (dueDate < today)
        {
            return MaintenanceStatus.Overdue;
        }

        if (dueDate <= today.AddDays(dueSoonDaysThreshold))
        {
            return MaintenanceStatus.DueSoon;
        }

        return MaintenanceStatus.Good;
    }

    public static int? GetDaysRemaining(DateTime? nextDueDate, DateTime referenceDate)
    {
        if (!nextDueDate.HasValue)
        {
            return null;
        }

        return (int)(nextDueDate.Value.Date - referenceDate.Date).TotalDays;
    }

    public static OverallHealthStatus CalculateOverallHealth(IEnumerable<MaintenanceStatus> statuses)
    {
        var statusList = statuses.Where(s => s != MaintenanceStatus.Disabled).ToList();

        if (!statusList.Any())
        {
            return OverallHealthStatus.Good;
        }

        if (statusList.Any(s => s == MaintenanceStatus.Overdue))
        {
            return OverallHealthStatus.Overdue;
        }

        if (statusList.Any(s => s == MaintenanceStatus.DueSoon))
        {
            return OverallHealthStatus.DueSoon;
        }

        return OverallHealthStatus.Good;
    }

    public static string FormatInterval(IntervalType intervalType, int intervalValue)
    {
        return intervalType switch
        {
            IntervalType.Days => intervalValue == 1 ? "Every day" : $"Every {intervalValue} days",
            IntervalType.Weeks => intervalValue == 1 ? "Every week" : $"Every {intervalValue} weeks",
            IntervalType.Months => intervalValue == 1 ? "Every month" : $"Every {intervalValue} months",
            IntervalType.Years => intervalValue == 1 ? "Every year" : $"Every {intervalValue} years",
            IntervalType.Custom => $"{intervalValue} days",
            IntervalType.OneTime => "One-time check",
            IntervalType.Disabled => "Disabled",
            _ => $"{intervalValue} {intervalType}"
        };
    }

    public static string FormatDaysRemaining(int? days)
    {
        if (!days.HasValue) return "Not scheduled";
        if (days.Value < 0) return $"{Math.Abs(days.Value)} day{(Math.Abs(days.Value) == 1 ? "" : "s")} overdue";
        if (days.Value == 0) return "Due today";
        if (days.Value == 1) return "Due tomorrow";
        return $"In {days.Value} days";
    }
}
