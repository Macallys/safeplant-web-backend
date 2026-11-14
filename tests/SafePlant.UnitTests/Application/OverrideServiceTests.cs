using FluentAssertions;
using Moq;
using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;
using SafetyActuation.Application.Services;
using SafetyActuation.Domain.Services;
using SharedKernel.Application;

namespace SafePlant.UnitTests.Application;

public sealed class OverrideServiceTests
{
    private readonly Mock<IAreaStore> _areas = new();
    private readonly Mock<IPlantOverrideLink> _plant = new();
    private readonly OverrideService _sut;

    public OverrideServiceTests()
    {
        _sut = new OverrideService(_areas.Object, _plant.Object);
    }

    [Fact]
    public async Task Override_sends_command_when_pair_and_area_are_valid()
    {
        var areaId = Guid.NewGuid();
        _areas.Setup(a => a.FindByIdAsync(areaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IndustrialArea(areaId, "Horno 1", "Nave A", DateTimeOffset.UtcNow));

        await _sut.OverrideAsync("Supervisor", "Mobile", areaId.ToString(), "Extractor", "On", CancellationToken.None);

        _plant.Verify(p => p.SendAsync(areaId, "Extractor", "On", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Override_rejects_Failed_runState_with_invalid_action()
    {
        var act = () => _sut.OverrideAsync(
            "Supervisor",
            "Mobile",
            Guid.NewGuid().ToString(),
            "Extractor",
            "Failed",
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("invalid_action");
        ex.Which.StatusCode.Should().Be(400);
        _plant.Verify(
            p => p.SendAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Override_returns_area_not_found_for_unknown_area_with_valid_pair()
    {
        var areaId = Guid.NewGuid();
        _areas.Setup(a => a.FindByIdAsync(areaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndustrialArea?)null);

        var act = () => _sut.OverrideAsync("Supervisor", "Mobile", areaId.ToString(), "Siren", "Off", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("area_not_found");
        ex.Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Override_rejects_PlantManager_Web_with_role_not_allowed()
    {
        var act = () => _sut.OverrideAsync(
            "PlantManager",
            "Web",
            Guid.NewGuid().ToString(),
            "Extractor",
            "On",
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("role_not_allowed");
        ex.Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Override_propagates_edge_unreachable_from_plant_link()
    {
        var areaId = Guid.NewGuid();
        _areas.Setup(a => a.FindByIdAsync(areaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IndustrialArea(areaId, "Horno 1", "Nave A", DateTimeOffset.UtcNow));
        _plant.Setup(p => p.SendAsync(areaId, "Barrier", "Deployed", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AppException("edge_unreachable", 503));

        var act = () => _sut.OverrideAsync(
            "Supervisor",
            "Mobile",
            areaId.ToString(),
            "Barrier",
            "Deployed",
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("edge_unreachable");
        ex.Which.StatusCode.Should().Be(503);
    }
}
