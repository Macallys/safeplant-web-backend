using IAM.Domain;

namespace IAM.Application;

public sealed record AccessSession(Guid AccountId, Guid SessionId, Role Role, Channel Channel);
