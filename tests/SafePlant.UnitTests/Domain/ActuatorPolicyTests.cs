using FluentAssertions;
using SafetyActuation.Domain.Services;

namespace SafePlant.UnitTests.Domain;

public sealed class ActuatorPolicyTests
{
    [Fact]
    public void Barrier_deploys_when_presence_is_Cleared()
    {
        var targets = ActuatorPolicy.Targets(
            co2Known: false,
            co2High: false,
            noiseKnown: false,
            noiseHigh: false,
            presence: ActuatorPolicy.Cleared);

        targets.Should().ContainSingle(t => t.Type == ActuatorPolicy.Barrier && t.RunState == ActuatorPolicy.Deployed);
    }

    [Fact]
    public void Barrier_retracts_when_presence_is_Detected()
    {
        var targets = ActuatorPolicy.Targets(
            co2Known: false,
            co2High: false,
            noiseKnown: false,
            noiseHigh: false,
            presence: ActuatorPolicy.Detected);

        targets.Should().ContainSingle(t => t.Type == ActuatorPolicy.Barrier && t.RunState == ActuatorPolicy.Retracted);
    }

    [Fact]
    public void Barrier_is_untouched_without_presence_reading()
    {
        var targets = ActuatorPolicy.Targets(
            co2Known: false,
            co2High: false,
            noiseKnown: false,
            noiseHigh: false,
            presence: null);

        targets.Should().BeEmpty();
    }

    [Fact]
    public void Extractor_turns_On_when_co2_is_high()
    {
        var targets = ActuatorPolicy.Targets(
            co2Known: true,
            co2High: true,
            noiseKnown: false,
            noiseHigh: false,
            presence: null);

        targets.Should().ContainSingle(t => t.Type == ActuatorPolicy.Extractor && t.RunState == ActuatorPolicy.On);
    }

    [Fact]
    public void Siren_turns_Off_when_noise_is_not_high()
    {
        var targets = ActuatorPolicy.Targets(
            co2Known: false,
            co2High: false,
            noiseKnown: true,
            noiseHigh: false,
            presence: null);

        targets.Should().ContainSingle(t => t.Type == ActuatorPolicy.Siren && t.RunState == ActuatorPolicy.Off);
    }

    [Theory]
    [InlineData("Extractor", "On", true)]
    [InlineData("Extractor", "Off", true)]
    [InlineData("Siren", "On", true)]
    [InlineData("Barrier", "Deployed", true)]
    [InlineData("Barrier", "Retracted", true)]
    [InlineData("Barrier", "Failed", false)]
    [InlineData("Extractor", "Failed", false)]
    [InlineData("Unknown", "On", false)]
    public void IsValidPair_matches_override_contract(string type, string runState, bool expected)
    {
        ActuatorPolicy.IsValidPair(type, runState).Should().Be(expected);
    }
}
