using Microsoft.EntityFrameworkCore;
using SafetyActuation.Domain.Entities;
using SafetyActuation.Domain.Interfaces;
using SafetyActuation.Domain.Services;
using SharedKernel.Application;

namespace SafetyActuation.Infrastructure.Persistence;

public sealed class EfSafetySyncStore : ISafetySyncStore
{
    private readonly SafetyActuationDbContext _db;

    public EfSafetySyncStore(SafetyActuationDbContext db)
    {
        _db = db;
    }

    public async Task UpsertAsync(SafetySync sync, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        if (sync.Severity is not null)
        {
            var exposure = await _db.Exposures.SingleOrDefaultAsync(entry => entry.AreaId == sync.AreaId, cancellationToken);
            if (exposure is null)
            {
                _db.Exposures.Add(ExposureState.Record(sync.AreaId, sync.Severity, sync.Highlighted));
            }
            else
            {
                exposure.Apply(sync.Severity, sync.Highlighted);
            }
        }

        if (sync.Alerts is not null)
        {
            foreach (var alert in sync.Alerts)
            {
                var existing = await _db.Alerts.SingleOrDefaultAsync(entry => entry.Id == alert.Id, cancellationToken);
                if (existing is null)
                {
                    var created = EnvironmentalAlert.Raise(alert.Id, sync.AreaId, alert.RaisedAt);
                    if (alert.WithdrawnAt is not null)
                    {
                        created.Withdraw(alert.WithdrawnAt.Value);
                    }

                    _db.Alerts.Add(created);
                    continue;
                }

                if (existing.AreaId != sync.AreaId)
                {
                    throw new AppException("invalid_safety", 422);
                }

                if (alert.WithdrawnAt is not null)
                {
                    existing.Withdraw(alert.WithdrawnAt.Value);
                }
            }
        }

        foreach (var actuator in sync.Actuators)
        {
            var byId = await _db.Actuators.SingleOrDefaultAsync(entry => entry.Id == actuator.Id, cancellationToken);
            if (byId is not null)
            {
                if (byId.AreaId != sync.AreaId || byId.Type != actuator.Type)
                {
                    throw new AppException("invalid_safety", 422);
                }

                byId.Apply(actuator.RunState, actuator.Mode, actuator.LastChangedAt);
                continue;
            }

            var byType = await _db.Actuators.SingleOrDefaultAsync(
                entry => entry.AreaId == sync.AreaId && entry.Type == actuator.Type,
                cancellationToken);
            if (byType is not null)
            {
                byType.Apply(actuator.RunState, actuator.Mode, actuator.LastChangedAt);
                continue;
            }

            _db.Actuators.Add(ActuatorState.Record(
                actuator.Id,
                sync.AreaId,
                actuator.Type,
                actuator.RunState,
                actuator.Mode,
                actuator.LastChangedAt));
        }

        foreach (var action in sync.Actions)
        {
            var existing = await _db.Actions.SingleOrDefaultAsync(entry => entry.Id == action.Id, cancellationToken);
            if (existing is not null)
            {
                continue;
            }

            _db.Actions.Add(ActuatorAction.Record(
                action.Id,
                sync.AreaId,
                action.Type,
                action.Succeeded,
                action.RecordedAt));
        }

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
