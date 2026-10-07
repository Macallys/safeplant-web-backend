namespace SharedKernel.Infrastructure;

public sealed record ApiError(string Code, string? CorrelationId);
