using DeviceEdgeManagement.Domain.Services;

namespace DeviceEdgeManagement.Domain.Interfaces;

public interface IPlantLocalStore
{
    Task ReplaceSheetAsync(PlantSheet sheet, CancellationToken cancellationToken);

    Task<PlantCredentialReplica?> FindCredentialAsync(string deviceId, CancellationToken cancellationToken);

    Task<PlantDeviceReplica?> FindDeviceAsync(string deviceId, CancellationToken cancellationToken);

    Task SaveReadingAsync(LocalReading reading, CancellationToken cancellationToken);

    Task EnqueueTelemetryAsync(LocalReading? reading, string body, CancellationToken cancellationToken);

    Task<OutboundMessage?> PeekTelemetryAsync(CancellationToken cancellationToken);

    Task DropAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<AreaLoopState>> ReadLoopAsync(CancellationToken cancellationToken);

    Task ApplyUpdateAsync(AreaUpdate update, string safetyBody, CancellationToken cancellationToken);

    Task<OutboundMessage?> PeekSafetyAsync(CancellationToken cancellationToken);

    Task<bool> CompleteSafetyAsync(long id, string body, IReadOnlyList<Guid> syncedActionIds, CancellationToken cancellationToken);
}
