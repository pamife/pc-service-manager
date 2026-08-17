namespace PcServiceManager.Core.Models;

public class ServiceHistoryCsvRecord
{
    public string ServiceDate { get; set; } = string.Empty;
    public string PcName { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string DurationMinutes { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CompletedCount { get; set; }
    public int SkippedCount { get; set; }
    public int NeedsAttentionCount { get; set; }
    public string OverallNote { get; set; } = string.Empty;
}
