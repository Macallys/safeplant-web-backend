using FluentAssertions;
using IAM.Application.Services;
using IAM.Domain.Entities;
using IAM.Domain.Interfaces;
using IAM.Domain.ValueObjects;
using Moq;
using SharedKernel.Application;
using SharedKernel.Domain;

namespace SafePlant.UnitTests.Application;

public sealed class SignInServiceTests
{
    private readonly Mock<IIamStore> _store = new();
    private readonly Mock<IPasswordHasher> _passwords = new();
    private readonly Mock<IAccessTokenIssuer> _tokens = new();
    private readonly Mock<IClock> _clock = new();
    private readonly SignInService _sut;

    public SignInServiceTests()
    {
        _clock.Setup(c => c.UtcNow).Returns(new DateTimeOffset(2026, 10, 29, 15, 0, 0, TimeSpan.Zero));
        _sut = new SignInService(_store.Object, _passwords.Object, _tokens.Object, _clock.Object, TimeSpan.FromHours(8));
    }

    [Fact]
    public async Task SignIn_returns_token_for_Supervisor_on_Mobile()
    {
        var account = new UserAccount(Guid.NewGuid(), "supervisor@safeplant.local", "hash", Role.Supervisor, enabled: true);
        _store.Setup(s => s.FindByEmailAsync("supervisor@safeplant.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _passwords.Setup(p => p.Matches("secret", "hash")).Returns(true);
        _tokens.Setup(t => t.Issue(account, It.IsAny<Session>())).Returns("jwt-token");

        var result = await _sut.SignIn("supervisor@safeplant.local", "secret", "Mobile", CancellationToken.None);

        result.AccessToken.Should().Be("jwt-token");
        result.Role.Should().Be("Supervisor");
        result.ExpiresAt.Should().Be(_clock.Object.UtcNow.AddHours(8));
        _store.Verify(s => s.AddSessionAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Once);
        _store.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignIn_rejects_bad_password_with_invalid_credentials()
    {
        var account = new UserAccount(Guid.NewGuid(), "supervisor@safeplant.local", "hash", Role.Supervisor, enabled: true);
        _store.Setup(s => s.FindByEmailAsync("supervisor@safeplant.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _passwords.Setup(p => p.Matches("wrong", "hash")).Returns(false);

        var act = () => _sut.SignIn("supervisor@safeplant.local", "wrong", "Mobile", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("invalid_credentials");
        ex.Which.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task SignIn_rejects_Supervisor_on_Web_with_role_not_allowed()
    {
        var account = new UserAccount(Guid.NewGuid(), "supervisor@safeplant.local", "hash", Role.Supervisor, enabled: true);
        _store.Setup(s => s.FindByEmailAsync("supervisor@safeplant.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _passwords.Setup(p => p.Matches("secret", "hash")).Returns(true);

        var act = () => _sut.SignIn("supervisor@safeplant.local", "secret", "Web", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("role_not_allowed");
        ex.Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task SignIn_rejects_disabled_account()
    {
        var account = new UserAccount(Guid.NewGuid(), "supervisor@safeplant.local", "hash", Role.Supervisor, enabled: false);
        _store.Setup(s => s.FindByEmailAsync("supervisor@safeplant.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _passwords.Setup(p => p.Matches("secret", "hash")).Returns(true);

        var act = () => _sut.SignIn("supervisor@safeplant.local", "secret", "Mobile", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("account_disabled");
        ex.Which.StatusCode.Should().Be(403);
    }
}
