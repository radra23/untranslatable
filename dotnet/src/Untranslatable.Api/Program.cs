using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Untranslatable.Api;
using Untranslatable.Shared.Monitoring;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Options;
using Untranslatable.Api.Monitoring;
using System;
using OpenTelemetry.Resources;

Metrics.App.Start.Add(1);

var builder = WebApplication.CreateBuilder(args);
builder.Environment.ConfigureContentPath();

// Add services to the container.
builder.Services.AddAllApplicationServices(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetryTracing(b => b
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("unstranslatable-dotnet"))
    .AddAspNetCoreInstrumentation()
    .AddJaegerExporter(o =>
    {
        var sp=b.GetServices().BuildServiceProvider();
        var ms=sp.GetRequiredService<IOptions<MonitoringSettings>>();
        o.AgentHost = ms.Value.Hostname;
        o.AgentPort = ms.Value.Port; // use port number here
        o.Endpoint = new Uri(ms.Value.CollectorEndpoint);
    })

    // The rest of your setup code goes here too
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.EnableTryItOutByDefault());
}

app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();

Metrics.App.Stop.Add(1);