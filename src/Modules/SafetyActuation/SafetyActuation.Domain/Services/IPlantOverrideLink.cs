namespace SafetyActuation.Domain.Services;

public interface IPlantOverrideLink
{
    Task SendAsync(Guid areaId, string actuatorType, string runState, CancellationToken cancellationToken);
}
