using FluentAssertions;
using Moq;
using PlantMonitoring.Application.Services;
using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;
using SharedKernel.Application;
using SharedKernel.Domain;

namespace SafePlant.UnitTests.Application;

public sealed class ThresholdServiceTests
{
    private readonly Mock<IAreaStore> _areas = new();
    private readonly Mock<IThresholdStore> _thresholds = new();
    private readonly Mock<IClock> _clock = new();
    private readonly ThresholdService _sut;
    private readonly Guid _areaId = Guid.NewGuid();

    public ThresholdServiceTests()
    {
        _clock.Setup(c => c.UtcNow).Returns(new DateTimeOffset(2026, 10, 29, 15, 0, 0, TimeSpan.Zero));
        _sut = new ThresholdService(_areas.Object, _thresholds.Object, _clock.Object, 1, 50000, 1, 140);
        _areas.Setup(a => a.FindByIdAsync(_areaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IndustrialArea(_areaId, "Horno 1", "Nave A", _clock.Object.UtcNow));
    }

    [Fact]
    public async Task Configure_adds_thresholds_for_Supervisor_Mobile()
    {
        _thresholds.Setup(t => t.FindByAreaIdAsync(_areaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AreaThresholds?)null);
        _thresholds.Setup(t => t.AddAsync(It.IsAny<AreaThresholds>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AreaThresholds t, CancellationToken _) => t);

        var result = await _sut.ConfigureAsync("Supervisor", "Mobile", _areaId.ToString(), 1000, 85, CancellationToken.None);

        result.Co2LimitPpm.Should().Be(1000);
        result.NoiseLimitDb.Should().Be(85);
    }

    [Fact]
    public async Task Configure_rejects_fractional_co2()
    {
        var act = () => _sut.ConfigureAsync("Supervisor", "Mobile", _areaId.ToString(), 1000.5m, 85, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("co2_limit_out_of_range");
        ex.Which.StatusCode.Should().Be(422);
    }

    [Fact]
    public async Task Configure_rejects_noise_above_140()
    {
        var act = () => _sut.ConfigureAsync("Supervisor", "Mobile", _areaId.ToString(), 1000, 141, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("noise_limit_out_of_range");
        ex.Which.StatusCode.Should().Be(422);
    }

    [Fact]
    public async Task Configure_prefers_co2_error_when_both_out_of_range()
    {
        var act = () => _sut.ConfigureAsync("Supervisor", "Mobile", _areaId.ToString(), 0, 0, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("co2_limit_out_of_range");
    }
}
