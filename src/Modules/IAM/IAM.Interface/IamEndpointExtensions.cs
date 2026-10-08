using IAM.Application;
using IAM.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IAM.Interface;

public static class IamEndpointExtensions
{
    public static IEndpointRouteBuilder MapIamEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/login", async (
            SignInBody body,
            SignInService signIn,
            CancellationToken cancellationToken) =>
        {
            var result = await signIn.SignIn(body.Email, body.Password, body.Channel, cancellationToken);
            return Results.Ok(new SignInResponse(result.AccessToken, result.Role, result.ExpiresAt));
        });

        app.MapPost("/api/v1/auth/recovery", async (
            RecoveryBody body,
            RecoveryService recovery,
            CancellationToken cancellationToken) =>
        {
            await recovery.RequestAsync(body.Email, cancellationToken);
            return Results.Accepted();
        });

        app.MapPost("/api/v1/auth/reset", async (
            ResetBody body,
            RecoveryService recovery,
            CancellationToken cancellationToken) =>
        {
            await recovery.ResetAsync(body.Token, body.Password, cancellationToken);
            return Results.NoContent();
        });

        var secured = app.MapGroup("").AddEndpointFilter<RequireSessionFilter>();

        secured.MapPost("/api/v1/auth/logout", async (
            HttpContext http,
            SessionAccess access,
            CancellationToken cancellationToken) =>
        {
            await access.LogoutAsync(Current(http), cancellationToken);
            return Results.NoContent();
        });

        secured.MapPost("/api/v1/users", async (
            CreateUserBody body,
            HttpContext http,
            DirectoryService directory,
            CancellationToken cancellationToken) =>
        {
            var account = await directory.CreateAsync(
                Current(http),
                body.Email,
                body.Password,
                body.Role,
                cancellationToken);
            return Results.Created($"/api/v1/users/{account.Id}", ToResponse(account));
        });

        secured.MapPatch("/api/v1/users/{accountId}/role", async (
            string accountId,
            AssignRoleBody body,
            HttpContext http,
            DirectoryService directory,
            CancellationToken cancellationToken) =>
        {
            var account = await directory.AssignRoleAsync(Current(http), accountId, body.Role, cancellationToken);
            return Results.Ok(ToResponse(account));
        });

        secured.MapPatch("/api/v1/users/{accountId}/enabled", async (
            string accountId,
            SetEnabledBody body,
            HttpContext http,
            DirectoryService directory,
            CancellationToken cancellationToken) =>
        {
            var account = await directory.SetEnabledAsync(Current(http), accountId, body.Enabled, cancellationToken);
            return Results.Ok(ToResponse(account));
        });

        secured.MapGet("/api/v1/users", async (
            HttpContext http,
            DirectoryService directory,
            CancellationToken cancellationToken) =>
        {
            var accounts = await directory.ListAsync(Current(http), cancellationToken);
            return Results.Ok(new UserListResponse(accounts.Select(ToResponse).ToArray()));
        });

        return app;
    }

    private static AccessSession Current(HttpContext http) =>
        (AccessSession)http.Items[RequireSessionFilter.ItemKey]!;

    private static UserAccountResponse ToResponse(UserAccount account) =>
        new(account.Id.ToString(), account.Email, account.Role.ToString(), account.Enabled);
}
