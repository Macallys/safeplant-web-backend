using IAM.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IAM.Infrastructure;

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

        return app;
    }

    public sealed record SignInBody(string? Email, string? Password, string? Channel);

    public sealed record SignInResponse(string AccessToken, string Role, DateTimeOffset ExpiresAt);
}
