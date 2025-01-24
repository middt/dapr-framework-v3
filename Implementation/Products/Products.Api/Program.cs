using Products.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Products.Domain.Entities;
using Dapr.Framework.Domain.Repositories;
using Dapr.Framework.Infrastructure.Repositories;
using Dapr.Client;
using Products.Application.Services;
using Dapr.Framework.Domain.Services;
using Dapr.Framework.Application.Services;
using Dapr.Framework.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Products.Infrastructure.Enrichers;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Dapr.Framework.Telemetry.Configuration;
using Dapr.Framework.Api.Configuration;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure telemetry using framework's configuration
builder.Services.AddHttpContextAccessor();
var serviceProvider = builder.Services.BuildServiceProvider();
builder.Services.AddFrameworkTelemetry(builder.Configuration, (loggerConfig, telemetryOptions) =>
{
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    loggerConfig.Enrich.With(new ProductsLogEnricher(
        customName: "x-custom-app-name",
        configuration: builder.Configuration,
        httpContextAccessor: httpContextAccessor
    ));
});

// Option 1: Use .NET Core Distributed Cache
builder.Services.AddNetCoreDistributedCache(services =>
{
    // Configure your preferred distributed cache implementation
    services.AddDistributedMemoryCache(); // Default in-memory
});

// Option 2: Use Dapr State Store Cache
// builder.Services.AddDaprStateStoreCache();

// Add Redis Configuration
builder.Services.AddRedis(builder.Configuration);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Add API Versioning using the custom configuration
builder.Services.AddCustomApiVersioning(apiTitle: "Products API");

// Add Controllers
builder.Services.AddControllers();

// Configure Entity Framework Core with In-Memory Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("ProductsDb"));

// Register DbContext as base type for generic repository
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

// Configure Dapr
builder.Services.AddDaprClient();
builder.Services.AddScoped<DaprClient>(sp => new DaprClientBuilder().Build());

// Register Repositories
builder.Services.AddScoped<ICRUDRepository<Product>, DaprCRUDRepository<Product>>();
builder.Services.AddScoped<ICRUDRepository<Order>, EfCRUDRepository<Order>>();
builder.Services.AddScoped<ICRUDRepository<OrderItem>, EfCRUDRepository<OrderItem>>();

// Register Services
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<IProductOrderService, ProductOrderService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Register Framework Services
builder.Services.AddScoped<ExternalApiService>();
builder.Services.AddScoped<IDistributedLockService, DaprDistributedLockService>();
// Register distributed lock service
// builder.services.AddScoped<IDistributedLockService, RedisDistributedLockService>();

builder.Services.AddScoped<ITransactionService, EfTransactionService>();

var app = builder.Build();

app.UseCustomApiVersioning();

app.UseCloudEvents();
app.MapSubscribeHandler();

// Map Controllers
app.MapControllers();

app.Run();
