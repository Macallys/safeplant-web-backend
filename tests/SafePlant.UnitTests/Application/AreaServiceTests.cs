using FluentAssertions;
using Moq;
using PlantMonitoring.Application.Services;
using PlantMonitoring.Domain.Entities;
using PlantMonitoring.Domain.Interfaces;
using SharedKernel.Application;
using SharedKernel.Domain;

namespace SafePlant.UnitTests.Application;

public sealed class AreaServiceTests
{
    private readonly Mock<IAreaStore> _store = new();
    private readonly Mock<IClock> _clock = new();
    private readonly AreaService _sut;

    public AreaServiceTests()
    {
        _clock.Setup(c => c.UtcNow).Returns(new DateTimeOffset(2026, 10, 29, 15, 0, 0, TimeSpan.Zero));
        _sut = new AreaService(_store.Object, _clock.Object);
    }

    [Fact]
    public async Task Create_returns_area_for_Supervisor_Mobile()
    {
        _store.Setup(s => s.FindByNameAsync("Horno 1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndustrialArea?)null);

        var area = await _sut.CreateAsync("Supervisor", "Mobile", "Horno 1", "Nave A", CancellationToken.None);

        area.Name.Should().Be("Horno 1");
        area.Location.Should().Be("Nave A");
        _store.Verify(s => s.AddAsync(It.IsAny<IndustrialArea>(), It.IsAny<CancellationToken>()), Times.Once);
        _store.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_rejects_duplicate_name()
    {
        _store.Setup(s => s.FindByNameAsync("Horno 1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IndustrialArea(Guid.NewGuid(), "Horno 1", "Nave A", _clock.Object.UtcNow));

        var act = () => _sut.CreateAsync("Supervisor", "Mobile", "Horno 1", "Nave A", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("area_name_duplicate");
        ex.Which.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Create_rejects_blank_name_with_invalid_area()
    {
        var act = () => _sut.CreateAsync("Supervisor", "Mobile", "  ", "Nave A", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("invalid_area");
        ex.Which.StatusCode.Should().Be(422);
    }

    [Fact]
    public async Task Create_rejects_PlantManager_with_role_not_allowed()
    {
        var act = () => _sut.CreateAsync("PlantManager", "Web", "Horno 1", "Nave A", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("role_not_allowed");
        ex.Which.StatusCode.Should().Be(403);
    }
}
