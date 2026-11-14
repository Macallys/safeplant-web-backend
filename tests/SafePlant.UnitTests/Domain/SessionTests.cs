using FluentAssertions;
using IAM.Domain.Entities;
using IAM.Domain.ValueObjects;

namespace SafePlant.UnitTests.Domain;

public sealed class SessionTests
{
    [Fact]
    public void IsOpen_is_true_before_expiry_and_without_close()
    {
        var now = new DateTimeOffset(2026, 10, 29, 15, 0, 0, TimeSpan.Zero);
        var session = Session.Open(Guid.NewGuid(), Guid.NewGuid(), Channel.Mobile, now.AddHours(8));

        session.IsOpen(now).Should().BeTrue();
    }

    [Fact]
    public void IsOpen_is_false_after_Close()
    {
        var now = new DateTimeOffset(2026, 10, 29, 15, 0, 0, TimeSpan.Zero);
        var session = Session.Open(Guid.NewGuid(), Guid.NewGuid(), Channel.Mobile, now.AddHours(8));
        session.Close(now);

        session.IsOpen(now).Should().BeFalse();
    }

    [Fact]
    public void IsOpen_is_false_when_expired()
    {
        var now = new DateTimeOffset(2026, 10, 29, 15, 0, 0, TimeSpan.Zero);
        var session = Session.Open(Guid.NewGuid(), Guid.NewGuid(), Channel.Mobile, now.AddMinutes(-1));

        session.IsOpen(now).Should().BeFalse();
    }
}
