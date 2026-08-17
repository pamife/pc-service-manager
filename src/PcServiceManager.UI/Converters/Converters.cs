using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using PcServiceManager.Core.Enums;

namespace PcServiceManager.UI.Converters;

public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is MaintenanceStatus maintStatus)
        {
            return maintStatus switch
            {
                MaintenanceStatus.Good => new SolidColorBrush(Color.FromRgb(16, 185, 129)),      // Emerald Green #10B981
                MaintenanceStatus.DueSoon => new SolidColorBrush(Color.FromRgb(245, 158, 11)),   // Amber Yellow #F59E0B
                MaintenanceStatus.Overdue => new SolidColorBrush(Color.FromRgb(239, 68, 68)),    // Red #EF4444
                MaintenanceStatus.Disabled => new SolidColorBrush(Color.FromRgb(107, 114, 128)), // Slate Gray #6B7280
                _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))
            };
        }

        if (value is OverallHealthStatus healthStatus)
        {
            return healthStatus switch
            {
                OverallHealthStatus.Good => new SolidColorBrush(Color.FromRgb(16, 185, 129)),
                OverallHealthStatus.DueSoon => new SolidColorBrush(Color.FromRgb(245, 158, 11)),
                OverallHealthStatus.Overdue => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))
            };
        }

        if (value is ServiceTaskStatus taskStatus)
        {
            return taskStatus switch
            {
                ServiceTaskStatus.Completed => new SolidColorBrush(Color.FromRgb(16, 185, 129)),
                ServiceTaskStatus.Skipped => new SolidColorBrush(Color.FromRgb(245, 158, 11)),
                ServiceTaskStatus.NeedsAttention => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                ServiceTaskStatus.NotApplicable => new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))
            };
        }

        return new SolidColorBrush(Color.FromRgb(107, 114, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class StatusToBackgroundBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is MaintenanceStatus status)
        {
            return status switch
            {
                MaintenanceStatus.Good => new SolidColorBrush(Color.FromArgb(35, 16, 185, 129)),
                MaintenanceStatus.DueSoon => new SolidColorBrush(Color.FromArgb(35, 245, 158, 11)),
                MaintenanceStatus.Overdue => new SolidColorBrush(Color.FromArgb(35, 239, 68, 68)),
                _ => new SolidColorBrush(Color.FromArgb(20, 107, 114, 128))
            };
        }

        if (value is OverallHealthStatus health)
        {
            return health switch
            {
                OverallHealthStatus.Good => new SolidColorBrush(Color.FromArgb(35, 16, 185, 129)),
                OverallHealthStatus.DueSoon => new SolidColorBrush(Color.FromArgb(35, 245, 158, 11)),
                OverallHealthStatus.Overdue => new SolidColorBrush(Color.FromArgb(35, 239, 68, 68)),
                _ => new SolidColorBrush(Color.FromArgb(20, 107, 114, 128))
            };
        }

        return new SolidColorBrush(Color.FromArgb(20, 107, 114, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool b = value is bool boolVal && boolVal;
        if (Invert) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility vis)
        {
            bool b = vis == Visibility.Visible;
            return Invert ? !b : b;
        }
        return false;
    }
}

public class NullToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNullOrEmpty = value == null || (value is string s && string.IsNullOrWhiteSpace(s));
        bool visible = Invert ? isNullOrEmpty : !isNullOrEmpty;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class DeviceTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DeviceType dt)
        {
            return dt switch
            {
                DeviceType.Desktop => "Desktop Tower / Custom PC",
                DeviceType.Laptop => "Laptop / Notebook",
                DeviceType.AllInOne => "All-in-One PC",
                DeviceType.Server => "Home Server / Workstation",
                _ => "Personal Computer"
            };
        }
        return "PC";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
