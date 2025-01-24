using Dapr.Framework.Domain.Common;
using Dapr.Framework.Domain.Repositories;
using Dapr.Framework.Infrastructure.Repositories;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Http;
using Microsoft.EntityFrameworkCore;
using Dapr.Client;
using Dapr.Framework.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add API Versioning
builder.Services.AddCustomApiVersioning();

// Add Dapr
builder.Services.AddDaprClient();

// Add Controllers
builder.Services.AddControllers();

// Add DbContext
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
// builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(EfGenericRepository<>));
// builder.Services.AddScoped(typeof(DaprGenericRepository<>));

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter())
    .WithMetrics(meterProviderBuilder =>
        meterProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add Dapr endpoints
app.UseCloudEvents();
app.MapSubscribeHandler();

// Use API Versioning
app.UseApiVersioning();

// Map Controllers
app.MapControllers();

app.Run();
