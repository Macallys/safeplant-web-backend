using DeviceEdgeManagement.Infrastructure;
using IAM.Infrastructure;
using IAM.Interface.Endpoints;
using PlantMonitoring.Infrastructure;
using SafetyActuation.Infrastructure;
using SharedKernel.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerUI;

LoadEnvFile();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSystemClock();
builder.Services.AddIam(builder.Configuration);
builder.Services.AddPlantMonitoring();
builder.Services.AddSafetyActuation();
builder.Services.AddDeviceEdge();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseApiErrors();

//if (app.Environment.IsDevelopment())
if(true){
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

app.MapIamEndpoints();

app.Run();

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
