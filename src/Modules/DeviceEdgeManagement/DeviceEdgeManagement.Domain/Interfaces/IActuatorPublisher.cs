namespace DeviceEdgeManagement.Domain.Interfaces;

public interface IActuatorPublisher
{
    Task<bool> PublishAsync(
        Guid areaId,
        string type,
        string runState,
        string mode,
        CancellationToken cancellationToken);
}
