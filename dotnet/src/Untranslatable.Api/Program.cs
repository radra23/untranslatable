using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
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

var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME")
    ?? "untranslatable-dotnet";
var serviceVersion = Environment.GetEnvironmentVariable("OTEL_SERVICE_VERSION")
    ?? "0.1.0";
var deploymentEnv = Environment.GetEnvironmentVariable("OTEL_DEPLOYMENT_ENVIRONMENT")
    ?? "local";

// Wire OTel logs into the .NET logging pipeline.
// ILogger<T> calls in controllers automatically flow to Loki via OTLP with
// trace_id/span_id stamped on each record when emitted inside an active span.
Action<ResourceBuilder> configureResource = r => r
    .AddService(serviceName, serviceVersion: serviceVersion)
    .AddAttributes(new Dictionary<string, object>
    {
        ["deployment.environment"] = deploymentEnv,
    });

builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    var rb = ResourceBuilder.CreateDefault();
    configureResource(rb);
    options.SetResourceBuilder(rb);
    options.AddOtlpExporter();
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(configureResource)
    .WithTracing(b => b
        .AddAspNetCoreInstrumentation()
        .AddSource(serviceName)
        .AddOtlpExporter())
    .WithMetrics(b => b
        .AddAspNetCoreInstrumentation()
        .AddMeter(Metrics.MeterName)
        .AddOtlpExporter());

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

try { Metrics.App.Stop.Add(1); }
catch { /* telemetry must never crash the application */ }

public partial class Program { }
