using IAM.Domain.ValueObjects;

namespace IAM.Domain.Entities;

public sealed class UserAccount
{
    private UserAccount()
    {
    }

    public UserAccount(Guid id, string email, string passwordHash, Role role, bool enabled)
    {
        Id = id;
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        Enabled = enabled;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = "";

    public string PasswordHash { get; private set; } = "";

    public Role Role { get; private set; }

    public bool Enabled { get; private set; }

    public bool Accepts(Channel channel) =>
        (Role == Role.PlantManager && channel == Channel.Web)
        || (Role == Role.Supervisor && channel == Channel.Mobile);

    public void ChangeRole(Role role) => Role = role;

    public void SetEnabled(bool enabled) => Enabled = enabled;

    public void ChangePassword(string passwordHash) => PasswordHash = passwordHash;
}
