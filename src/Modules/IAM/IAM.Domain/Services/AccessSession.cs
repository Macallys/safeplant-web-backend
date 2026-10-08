using IAM.Domain.ValueObjects;

namespace IAM.Domain.Services;

public sealed record AccessSession(Guid AccountId, Guid SessionId, Role Role, Channel Channel);
