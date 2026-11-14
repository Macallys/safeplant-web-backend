using FluentAssertions;
using SafetyActuation.Domain.Services;

namespace SafePlant.UnitTests.Domain;

public sealed class ExposurePolicyTests
{
    [Fact]
    public void Severity_is_None_when_neither_measure_reaches_limit()
    {
        ExposurePolicy.Severity(800, 1000, 70, 85).Should().Be(ExposurePolicy.None);
    }

    [Fact]
    public void Severity_is_None_when_readings_are_missing()
    {
        ExposurePolicy.Severity(null, 1000, null, 85).Should().Be(ExposurePolicy.None);
    }

    [Fact]
    public void Severity_is_Medium_when_only_co2_is_high()
    {
        ExposurePolicy.Severity(1200, 1000, 70, 85).Should().Be(ExposurePolicy.Medium);
    }

    [Fact]
    public void Severity_is_Medium_when_only_noise_is_high()
    {
        ExposurePolicy.Severity(800, 1000, 90, 85).Should().Be(ExposurePolicy.Medium);
    }

    [Fact]
    public void Severity_is_High_when_both_are_at_or_above_limit()
    {
        ExposurePolicy.Severity(1000, 1000, 85, 85).Should().Be(ExposurePolicy.High);
    }

    [Theory]
    [InlineData(ExposurePolicy.High, true)]
    [InlineData(ExposurePolicy.Medium, false)]
    [InlineData(ExposurePolicy.None, false)]
    public void Highlighted_is_true_only_for_High(string severity, bool expected)
    {
        ExposurePolicy.Highlighted(severity).Should().Be(expected);
    }
}
