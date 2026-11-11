using DeviceEdgeManagement.Domain.Entities;

namespace DeviceEdgeManagement.Domain.Interfaces;

public interface IDeviceCredentialStore
{
    Task<DeviceCredential?> FindByDeviceIdAsync(string deviceId, CancellationToken cancellationToken);

    Task<IReadOnlyList<DeviceCredential>> ListAsync(CancellationToken cancellationToken);

    Task AddAsync(DeviceCredential credential, CancellationToken cancellationToken);

    Task RemoveAsync(DeviceCredential credential, CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}
