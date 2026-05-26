using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Untranslatable.Api;
using Untranslatable.Api.Monitoring;
using Untranslatable.Shared.Monitoring;

var builder = WebApplication.CreateBuilder(args);
builder.Environment.ConfigureContentPath();

builder.Services.AddAllApplicationServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

const string serviceName = "untranslatable-dotnet";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName))
    .WithTracing(b => b
        .AddAspNetCoreInstrumentation()
        .AddSource(serviceName)
        .AddOtlpExporter())
    .WithMetrics(b => b
        .AddAspNetCoreInstrumentation()
        .AddMeter(Metrics.MeterName)
        .AddOtlpExporter());

// Facade registered with a factory so that a resolve-time failure (e.g.
// TracerProvider missing because OTel registration was skipped) falls back
// gracefully to the no-op instead of crashing on first request.
builder.Services.AddSingleton<IWordsTelemetry>(sp =>
{
    try
    {
        var provider = sp.GetRequiredService<TracerProvider>();
        return new OtelWordsTelemetry(provider.GetTracer(serviceName));
    }
    catch (Exception ex)
    {
        sp.GetService<ILogger<IWordsTelemetry>>()
          ?.LogWarning(ex, "[telemetry] TracerProvider unavailable — using no-op telemetry.");
        return NullWordsTelemetry.Instance;
    }
});

// App-start counter — guarded so a metrics-provider failure never aborts startup.
try { Metrics.App.Start.Add(1); }
catch { /* telemetry must never crash the application */ }

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.EnableTryItOutByDefault());
}

app.MapHealthChecks("/healthz");
app.UseAuthorization();
app.MapControllers();

app.Run();

// App-stop counter — guarded for the same reason.
try { Metrics.App.Stop.Add(1); }
catch { /* telemetry must never crash the application */ }

// Required by WebApplicationFactory<Program> in integration tests.
public partial class Program { }
