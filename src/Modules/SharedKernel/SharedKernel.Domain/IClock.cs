namespace SharedKernel.Domain;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
