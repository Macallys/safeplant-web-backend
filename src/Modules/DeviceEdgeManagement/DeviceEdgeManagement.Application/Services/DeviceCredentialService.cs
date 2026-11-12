using System.Security.Cryptography;
using DeviceEdgeManagement.Domain.Entities;
using DeviceEdgeManagement.Domain.Interfaces;
using DeviceEdgeManagement.Domain.Services;
using PlantMonitoring.Domain.Interfaces;
using PlantMonitoring.Domain.Services;
using SharedKernel.Application;
using SharedKernel.Domain;

namespace DeviceEdgeManagement.Application.Services;

public sealed class DeviceCredentialService : IDeviceCredentialService
{
    private const int DeviceIdMaxLength = 64;
    private const int SecretByteLength = 32;

    private readonly IDeviceCredentialStore _credentials;
    private readonly IAreaStore _areas;
    private readonly IDeviceStore _devices;
    private readonly IDeviceService _association;
    private readonly ISecretHasher _secrets;
    private readonly IClock _clock;
    private readonly int _credentialDays;

    public DeviceCredentialService(
        IDeviceCredentialStore credentials,
        IAreaStore areas,
        IDeviceStore devices,
        IDeviceService association,
        ISecretHasher secrets,
        IClock clock,
        int credentialDays)
    {
        _credentials = credentials;
        _areas = areas;
        _devices = devices;
        _association = association;
        _secrets = secrets;
        _clock = clock;
        _credentialDays = credentialDays;
    }

    public async Task<IssuedCredential> IssueAsync(
        string role,
        string channel,
        string? deviceId,
        string? areaId,
        string? kind,
        CancellationToken cancellationToken)
    {
        RequireSupervisor(role, channel);
        var parsedDeviceId = RequireDeviceId(deviceId);
        var pair = RequirePair(areaId, kind);
        if (pair is not null)
        {
            await RequireAreaAsync(pair.AreaId, cancellationToken);
        }

        if (await _credentials.FindByDeviceIdAsync(parsedDeviceId, cancellationToken) is not null
            || await _devices.FindByDeviceIdAsync(parsedDeviceId, cancellationToken) is not null)
        {
            throw new AppException("device_already_exists", 409);
        }

        var issuedAt = _clock.UtcNow;
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(SecretByteLength));
        var credential = new DeviceCredential(
            parsedDeviceId,
            _secrets.Hash(secret),
            issuedAt,
            issuedAt.AddDays(_credentialDays));
        await _credentials.AddAsync(credential, cancellationToken);

        if (pair is not null)
        {
            try
            {
                await _association.AssociateAsync(
                    role,
                    channel,
                    pair.AreaId,
                    parsedDeviceId,
                    pair.Kind,
                    cancellationToken);
            }
            catch
            {
                await _credentials.RemoveAsync(credential, cancellationToken);
                throw;
            }
        }

        return new IssuedCredential(credential.DeviceId, credential.IssuedAt, credential.ExpiresAt, secret);
    }

    public async Task<IReadOnlyList<DeviceListItem>> ListAsync(
        string role,
        string channel,
        CancellationToken cancellationToken)
    {
        RequireSupervisor(role, channel);
        var credentials = await _credentials.ListAsync(cancellationToken);
        var associations = await _devices.ListAsync(cancellationToken);
        var byDeviceId = associations.ToDictionary(device => device.DeviceId, StringComparer.Ordinal);
        return credentials
            .Select(credential =>
            {
                byDeviceId.TryGetValue(credential.DeviceId, out var association);
                return new DeviceListItem(
                    credential.DeviceId,
                    association?.AreaId,
                    association?.Kind,
                    credential.IssuedAt,
                    credential.ExpiresAt,
                    credential.RevokedAt,
                    credential.LastSeenAt);
            })
            .ToArray();
    }

    public async Task<RevokedCredential> RevokeAsync(
        string role,
        string channel,
        string? deviceId,
        CancellationToken cancellationToken)
    {
        RequireSupervisor(role, channel);
        var credential = await _credentials.FindByDeviceIdAsync(deviceId ?? "", cancellationToken);
        if (credential is null)
        {
            throw new AppException("device_not_found", 404);
        }

        if (credential.RevokedAt is null)
        {
            credential.Revoke(_clock.UtcNow);
            await _credentials.SaveAsync(cancellationToken);
        }

        return new RevokedCredential(credential.DeviceId, credential.RevokedAt!.Value);
    }

    private async Task RequireAreaAsync(string areaId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(areaId, out var id))
        {
            throw new AppException("area_not_found", 422);
        }

        var area = await _areas.FindByIdAsync(id, cancellationToken);
        if (area is null)
        {
            throw new AppException("area_not_found", 422);
        }
    }

    private static string RequireDeviceId(string? deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new AppException("invalid_device", 422);
        }

        var trimmed = deviceId.Trim();
        if (trimmed.Length is < 1 or > DeviceIdMaxLength)
        {
            throw new AppException("invalid_device", 422);
        }

        return trimmed;
    }

    private static DevicePair? RequirePair(string? areaId, string? kind)
    {
        var hasArea = !string.IsNullOrWhiteSpace(areaId);
        var hasKind = !string.IsNullOrWhiteSpace(kind);
        if (hasArea != hasKind)
        {
            throw new AppException("invalid_device", 422);
        }

        if (!hasKind)
        {
            return null;
        }

        if (kind is not ("Sensor" or "Actuator"))
        {
            throw new AppException("invalid_device", 422);
        }

        return new DevicePair(areaId!, kind);
    }

    private static void RequireSupervisor(string role, string channel)
    {
        if (role != "Supervisor" || channel != "Mobile")
        {
            throw new AppException("role_not_allowed", 403);
        }
    }

    private sealed record DevicePair(string AreaId, string Kind);
}
