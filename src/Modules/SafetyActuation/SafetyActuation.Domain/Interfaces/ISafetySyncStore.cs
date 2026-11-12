using SafetyActuation.Domain.Services;

namespace SafetyActuation.Domain.Interfaces;

public interface ISafetySyncStore
{
    Task UpsertAsync(SafetySync sync, CancellationToken cancellationToken);
}