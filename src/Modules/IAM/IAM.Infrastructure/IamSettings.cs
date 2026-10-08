namespace IAM.Infrastructure;

public sealed class IamSettings
{
    public int SessionHours { get; set; } = 8;

    public int RecoveryHours { get; set; } = 1;

    public string SigningKey { get; set; } = "";

    public IamSeedSettings Seed { get; set; } = new();
}

public sealed class IamSeedSettings
{
    public string Email { get; set; } = "";

    public string Password { get; set; } = "";
}
