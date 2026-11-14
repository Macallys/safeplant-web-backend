using FluentAssertions;
using IAM.Domain.Entities;
using IAM.Domain.ValueObjects;

namespace SafePlant.UnitTests.Domain;

public sealed class UserAccountTests
{
    [Fact]
    public void PlantManager_accepts_only_Web()
    {
        var account = new UserAccount(Guid.NewGuid(), "manager@safeplant.local", "hash", Role.PlantManager, enabled: true);

        account.Accepts(Channel.Web).Should().BeTrue();
        account.Accepts(Channel.Mobile).Should().BeFalse();
    }

    [Fact]
    public void Supervisor_accepts_only_Mobile()
    {
        var account = new UserAccount(Guid.NewGuid(), "supervisor@safeplant.local", "hash", Role.Supervisor, enabled: true);

        account.Accepts(Channel.Mobile).Should().BeTrue();
        account.Accepts(Channel.Web).Should().BeFalse();
    }

    [Fact]
    public void Email_is_normalized_to_lower_trimmed()
    {
        var account = new UserAccount(Guid.NewGuid(), "  Admin@SafePlant.Local ", "hash", Role.PlantManager, enabled: true);

        account.Email.Should().Be("admin@safeplant.local");
    }
}
