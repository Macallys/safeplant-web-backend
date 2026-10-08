using IAM.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IAM.Interface;

internal sealed class RequireSessionFilter : IEndpointFilter
{
    public const string ItemKey = "iam-session";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var access = context.HttpContext.RequestServices.GetRequiredService<SessionAccess>();
        var header = context.HttpContext.Request.Headers.Authorization.ToString();
        var bearer = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header["Bearer ".Length..].Trim()
            : null;

        var session = await access.RequireAsync(bearer, context.HttpContext.RequestAborted);
        context.HttpContext.Items[ItemKey] = session;
        return await next(context);
    }
}
