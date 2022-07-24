using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Untranslatable.Api;
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.EnableTryItOutByDefault());
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

Metrics.App.Stop.Add(1);