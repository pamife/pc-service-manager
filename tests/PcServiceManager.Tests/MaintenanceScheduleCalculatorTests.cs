using FluentAssertions;
using PcServiceManager.Core.Enums;
using PcServiceManager.Core.Services;
using Xunit;

namespace PcServiceManager.Tests;

public class MaintenanceScheduleCalculatorTests
{
    [Fact]
    public void CalculateNextDueDate_ShouldAddCorrectDays_ForDaysInterval()
    {
        var baseDate = new DateTime(2026, 8, 1);
        var result = MaintenanceScheduleCalculator.CalculateNextDueDate(baseDate, IntervalType.Days, 14);

        result.Should().Be(new DateTime(2026, 8, 15));
    }

    [Fact]
    public void CalculateNextDueDate_ShouldAddCorrectDays_ForWeeksInterval()
    {
        var baseDate = new DateTime(2026, 8, 1);
        var result = MaintenanceScheduleCalculator.CalculateNextDueDate(baseDate, IntervalType.Weeks, 2);

        result.Should().Be(new DateTime(2026, 8, 15));
    }

    [Fact]
    public void CalculateNextDueDate_ShouldAddCorrectMonths_ForMonthsInterval()
    {
        var baseDate = new DateTime(2026, 1, 15);
        var result = MaintenanceScheduleCalculator.CalculateNextDueDate(baseDate, IntervalType.Months, 3);

        result.Should().Be(new DateTime(2026, 4, 15));
    }

    [Fact]
    public void CalculateNextDueDate_ShouldAddCorrectYears_ForYearsInterval()
    {
        var baseDate = new DateTime(2026, 8, 17);
        var result = MaintenanceScheduleCalculator.CalculateNextDueDate(baseDate, IntervalType.Years, 1);

        result.Should().Be(new DateTime(2027, 8, 17));
    }

    [Fact]
    public void CalculateNextDueDate_ShouldReturnNull_ForOneTimeOrDisabled()
    {
        var baseDate = new DateTime(2026, 8, 17);
        MaintenanceScheduleCalculator.CalculateNextDueDate(baseDate, IntervalType.OneTime, 1).Should().BeNull();
        MaintenanceScheduleCalculator.CalculateNextDueDate(baseDate, IntervalType.Disabled, 1).Should().BeNull();
    }

    [Theory]
    [InlineData(2026, 8, 10, MaintenanceStatus.Overdue)]   // 7 days in past
    [InlineData(2026, 8, 17, MaintenanceStatus.DueSoon)]   // Today (within 7 days threshold)
    [InlineData(2026, 8, 24, MaintenanceStatus.DueSoon)]   // Exactly 7 days ahead
    [InlineData(2026, 8, 25, MaintenanceStatus.Good)]      // 8 days ahead (> 7 days threshold)
    public void CalculateTaskStatus_ShouldReturnExpectedStatus(int year, int month, int day, MaintenanceStatus expectedStatus)
    {
        var referenceDate = new DateTime(2026, 8, 17);
        var nextDueDate = new DateTime(year, month, day);

        var status = MaintenanceScheduleCalculator.CalculateTaskStatus(
            nextDueDate,
            lastPerformedDate: new DateTime(2026, 7, 1),
            intervalType: IntervalType.Months,
            isEnabled: true,
            dueSoonDaysThreshold: 7,
            referenceDate: referenceDate);

        status.Should().Be(expectedStatus);
    }

    [Fact]
    public void CalculateTaskStatus_ShouldReturnDisabled_WhenDisabled()
    {
        var referenceDate = new DateTime(2026, 8, 17);
        var nextDueDate = new DateTime(2026, 8, 10);

        var status = MaintenanceScheduleCalculator.CalculateTaskStatus(
            nextDueDate,
            lastPerformedDate: null,
            intervalType: IntervalType.Disabled,
            isEnabled: false,
            dueSoonDaysThreshold: 7,
            referenceDate: referenceDate);

        status.Should().Be(MaintenanceStatus.Disabled);
    }

    [Fact]
    public void CalculateOverallHealth_ShouldPrioritizeOverdue()
    {
        var statuses = new[] { MaintenanceStatus.Good, MaintenanceStatus.DueSoon, MaintenanceStatus.Overdue };
        var overall = MaintenanceScheduleCalculator.CalculateOverallHealth(statuses);

        overall.Should().Be(OverallHealthStatus.Overdue);
    }

    [Fact]
    public void CalculateOverallHealth_ShouldReturnDueSoon_WhenNoOverdue()
    {
        var statuses = new[] { MaintenanceStatus.Good, MaintenanceStatus.DueSoon, MaintenanceStatus.Good };
        var overall = MaintenanceScheduleCalculator.CalculateOverallHealth(statuses);

        overall.Should().Be(OverallHealthStatus.DueSoon);
    }

    [Fact]
    public void CalculateOverallHealth_ShouldReturnGood_WhenAllGoodOrDisabled()
    {
        var statuses = new[] { MaintenanceStatus.Good, MaintenanceStatus.Disabled, MaintenanceStatus.Good };
        var overall = MaintenanceScheduleCalculator.CalculateOverallHealth(statuses);

        overall.Should().Be(OverallHealthStatus.Good);
    }
}
