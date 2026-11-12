using DeviceEdgeManagement.Infrastructure;
using SafetyActuation.Infrastructure;
using SharedKernel.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerUI;

LoadEnvFile();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSystemClock();
builder.Services.AddSafetyActuation();
builder.Services.AddDeviceEdge();
builder.Services.AddDeviceEdgePlant(builder.Configuration);

var app = builder.Build();

app.UseApiErrors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

if (ListensOnHttps(builder.Configuration["ASPNETCORE_URLS"]))
{
    app.UseHttpsRedirection();
}

app.Run();

static bool ListensOnHttps(string? urls) =>
    urls?.Contains("https://", StringComparison.OrdinalIgnoreCase) == true;

static void LoadEnvFile()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null)
    {
        var path = Path.Combine(directory.FullName, ".env");
        if (File.Exists(path))
        {
            DotNetEnv.Env.NoClobber().Load(path);
            return;
        }

        directory = directory.Parent;
    }
}
