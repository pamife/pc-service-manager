namespace PcServiceManager.Core.Models;

public class ScannedFolder
{
    public string Path { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long Bytes { get; set; }
    public int FileCount { get; set; }
    public string FormattedSize => FormatBytes(Bytes);
    public bool IsSelected { get; set; } = true;

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}

public class TempFileScanResult
{
    public List<ScannedFolder> Locations { get; set; } = new();
    public long TotalBytes => Locations.Where(l => l.IsSelected).Sum(l => l.Bytes);
    public int TotalFiles => Locations.Where(l => l.IsSelected).Sum(l => l.FileCount);
    public string FormattedTotalSize
    {
        get
        {
            var bytes = TotalBytes;
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        }
    }
}
