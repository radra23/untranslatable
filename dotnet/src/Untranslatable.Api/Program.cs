using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Untranslatable.Api;
using Untranslatable.Api.Monitoring;
using Untranslatable.Shared.Monitoring;

Metrics.App.Start.Add(1);

var builder = WebApplication.CreateBuilder(args);
builder.Environment.ConfigureContentPath();

// Add services to the container.
builder.Services.AddAllApplicationServices(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var serviceName = "unstranslatable-dotnet";
var resource = ResourceBuilder.CreateDefault().AddService(serviceName);
builder.Services.AddOpenTelemetryTracing(b => b
    .SetResourceBuilder(resource)
    .AddSource(serviceName)
    .AddAspNetCoreInstrumentation()
    .AddJaegerExporter(o =>
    {
        var sp = b.GetServices().BuildServiceProvider();
        var ms = sp.GetRequiredService<IOptions<MonitoringSettings>>();
        o.AgentHost = ms.Value.Hostname;
        o.AgentPort = ms.Value.Port;
        o.Endpoint = new Uri(ms.Value.CollectorEndpoint);
    })
).AddSingleton(TracerProvider.Default.GetTracer(serviceName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.EnableTryItOutByDefault());
}

app.UseAuthorization();

app.MapControllers();

app.Run();

Metrics.App.Stop.Add(1);