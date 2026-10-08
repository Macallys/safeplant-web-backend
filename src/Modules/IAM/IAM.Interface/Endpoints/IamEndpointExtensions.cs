using IAM.Domain.Services;
using IAM.Interface.Assemblers;
using IAM.Interface.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IAM.Interface.Endpoints;

public static class IamEndpointExtensions
{
    public static IEndpointRouteBuilder MapIamEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/login", async (
            SignInBody body,
            ISignInService signIn,
            CancellationToken cancellationToken) =>
        {
            var result = await signIn.SignIn(body.Email, body.Password, body.Channel, cancellationToken);
            return Results.Ok(IamAssembler.ToSignInResponse(result));
        });

        app.MapPost("/api/v1/auth/recovery", async (
            RecoveryBody body,
            IRecoveryService recovery,
            CancellationToken cancellationToken) =>
        {
            await recovery.RequestAsync(body.Email, cancellationToken);
            return Results.Accepted();
        });

        app.MapPost("/api/v1/auth/reset", async (
            ResetBody body,
            IRecoveryService recovery,
            CancellationToken cancellationToken) =>
        {
            await recovery.ResetAsync(body.Token, body.Password, cancellationToken);
            return Results.NoContent();
        });

        var secured = app.MapGroup("").AddEndpointFilter<RequireSessionFilter>();

        secured.MapPost("/api/v1/auth/logout", async (
            HttpContext http,
            ISessionAccess access,
            CancellationToken cancellationToken) =>
        {
            await access.LogoutAsync(Current(http), cancellationToken);
            return Results.NoContent();
        });

        secured.MapPost("/api/v1/users", async (
            CreateUserBody body,
            HttpContext http,
            IDirectoryService directory,
            CancellationToken cancellationToken) =>
        {
            var account = await directory.CreateAsync(
                Current(http),
                body.Email,
                body.Password,
                body.Role,
                cancellationToken);
            return Results.Created($"/api/v1/users/{account.Id}", IamAssembler.ToUserAccount(account));
        });

        secured.MapPatch("/api/v1/users/{accountId}/role", async (
            string accountId,
            AssignRoleBody body,
            HttpContext http,
            IDirectoryService directory,
            CancellationToken cancellationToken) =>
        {
            var account = await directory.AssignRoleAsync(Current(http), accountId, body.Role, cancellationToken);
            return Results.Ok(IamAssembler.ToUserAccount(account));
        });

        secured.MapPatch("/api/v1/users/{accountId}/enabled", async (
            string accountId,
            SetEnabledBody body,
            HttpContext http,
            IDirectoryService directory,
            CancellationToken cancellationToken) =>
        {
            var account = await directory.SetEnabledAsync(Current(http), accountId, body.Enabled, cancellationToken);
            return Results.Ok(IamAssembler.ToUserAccount(account));
        });

        secured.MapGet("/api/v1/users", async (
            HttpContext http,
            IDirectoryService directory,
            CancellationToken cancellationToken) =>
        {
            var accounts = await directory.ListAsync(Current(http), cancellationToken);
            return Results.Ok(IamAssembler.ToUserList(accounts));
        });

        return app;
    }

    private static AccessSession Current(HttpContext http) =>
        (AccessSession)http.Items[RequireSessionFilter.ItemKey]!;
}
