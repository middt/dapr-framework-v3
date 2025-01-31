# Dapr Framework with Clean Architecture

This project demonstrates a modern .NET Core framework implementation using Clean Architecture principles, Domain-Driven Design (DDD), and cloud-native technologies including Dapr and OpenTelemetry.

## Project Structure

The solution follows Clean Architecture principles with the following structure:

### Framework Layer (`Framework.sln`)
- **Dapr.Framework.Domain**: Core domain entities, interfaces, and business rules
- **Dapr.Framework.Application**: Application services, DTOs, and business logic
- **Dapr.Framework.Infrastructure**: Infrastructure implementations (Dapr, EF Core)
- **Dapr.Framework.Api**: Base API components and shared endpoints
- **Dapr.Framework.Telemetry**: Telemetry and observability infrastructure

### Implementation Example (`Products.sln`)
- **Products.Domain**: Product-specific domain entities and interfaces
- **Products.Application**: Product-specific business logic and services
- **Products.Infrastructure**: Product-specific infrastructure implementations
- **Products.Api**: Product API endpoints and configuration

## Features

### Clean Architecture
- Separation of concerns through layered architecture
- Domain-driven design principles
- Repository pattern for data access abstraction

### Dapr Integration
- State management using Dapr state store
- Service-to-service communication
- Pub/sub messaging capabilities
- Secrets management

### Telemetry and Observability
- Distributed tracing with multiple provider options
- Structured logging with Serilog and custom enrichers
- Metrics collection and export
- Multiple exporter options (Zipkin, OTLP, Elastic, OpenObserve)

## Configuration Guide

### Telemetry Configuration

The framework supports flexible telemetry configuration through `appsettings.json`:

```json
{
  "Telemetry": {
    "ServiceName": "YourServiceName",
    "ServiceVersion": "1.0.0",
    "Environment": "Development",    // Environment name for log enrichment
    "TraceProvider": "zipkin",      // Options: zipkin, otlp, elastic, openobserve
    "LogProvider": "console",       // Options: console, file
    "Logging": {
      "Enabled": true,
      "FilePath": "logs/app.log",
      "MinimumLevel": "Information"  // Options: Trace, Debug, Information, Warning, Error, Critical
    },
    "Zipkin": {
      "Endpoint": "http://localhost:9411/api/v2/spans"
    },
    "Elastic": {
      "Endpoint": "http://localhost:8200",
      "Username": "",
      "Password": ""
    },
    "Otlp": {
      "Endpoint": "http://localhost:4317"
    },
    "OpenObserve": {
      "Endpoint": "http://localhost:5080/api/default",
      "Username": "",
      "Password": "",
      "Organization": "default",
      "Stream": "default"
    }
  }
}
```

#### Custom Log Enrichment
The framework supports custom log enrichers to add additional context to your logs. Here's how to implement a custom enricher:

1. Create a class that implements `ILogEventEnricher`:
```csharp
public class CustomLogEnricher : ILogEventEnricher
{
    private readonly string _applicationName;
    private readonly string _environment;
    private readonly string _version;

    public CustomLogEnricher(
        string applicationName,
        string environment,
        string version)
    {
        _applicationName = applicationName;
        _environment = environment;
        _version = version;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Add custom properties to your logs
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ApplicationName", _applicationName));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Environment", _environment));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Version", _version));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MachineName", Environment.MachineName));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ProcessId", Environment.ProcessId));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadId", Environment.CurrentManagedThreadId));
    }
}
```

2. Register your enricher in `Program.cs`:
```csharp
builder.Services.AddFrameworkTelemetry(builder.Configuration, (loggerConfig, telemetryOptions) =>
{
    loggerConfig.Enrich.With(new CustomLogEnricher(
        applicationName: telemetryOptions.ServiceName,
        environment: telemetryOptions.Environment,
        version: telemetryOptions.ServiceVersion
    ));
});
```

3. Configure logging in `appsettings.json`:
```json
{
  "Telemetry": {
    "ServiceName": "YourService",
    "ServiceVersion": "1.0.0",
    "Environment": "Development",
    "LogProvider": "file",           // Options: console, file
    "Logging": {
      "Enabled": true,
      "FilePath": "logs/app.log",
      "MinimumLevel": "Information"
    }
  }
}
```

The enriched logs will be output in JSON format with your custom properties:
```json
{
  "@t": "2024-12-10T12:21:13.1234567Z",
  "@m": "Request processed successfully",
  "@l": "Information",
  "ApplicationName": "YourService",
  "Environment": "Development",
  "Version": "1.0.0",
  "MachineName": "HOST123",
  "ProcessId": 1234,
  "ThreadId": 5
}
```

#### Custom Log Enrichment (Default)
The framework includes a custom log enricher that automatically adds the following context to all log entries:
- Application Name: Name of the service
- Environment: Runtime environment (e.g., Development, Staging, Production)
- Machine Name: Name of the host machine
- Process ID: Current process identifier
- Thread ID: Current thread identifier
- Timestamp: UTC timestamp of the log entry

Example enriched log output:
```
[14:30:45 INF] User login successful Application:AuthService Environment:Production Machine:HOST123 Process:1234 Thread:5
```

#### Trace Providers
- **Zipkin**: Default distributed tracing system
- **OTLP**: OpenTelemetry Protocol exporter
- **Elastic**: Export to Elastic APM
- **OpenObserve**: Export to OpenObserve platform

#### Log Providers
- **Console**: Output logs to console with enriched context
- **File**: Write logs to file with rotation
  - Daily log rotation
  - 31-day retention period
  - Structured logging format
  - Enriched with custom context

## Getting Started

1. **Prerequisites**
   - .NET 9.0 SDK
   - Dapr CLI and runtime
   - Docker (for running infrastructure components)

2. **Setup Infrastructure**
   ```bash
   # Start Redis for state store
   docker run -d -p 6379:6379 redis

   # Start Zipkin for tracing
   docker run -d -p 9411:9411 openzipkin/zipkin
   ```

3. **Initialize Dapr**
   ```bash
   dapr init
   ```

4. **Run the Application**
   ```bash
   cd Implementation/Products/Products.Api
   dapr run --app-id products-api --app-port 5000 dotnet run
   ```

## Using the Framework

1. **Create a New Service**
   - Create a new solution using the provided structure
   - Reference the Framework projects
   - Implement domain entities and business logic

2. **Configure Telemetry**
   ```csharp
   // Program.cs
   builder.Services.AddFrameworkTelemetry(builder.Configuration);
   ```

3. **Use Repository Pattern**
   ```csharp
   // Register repositories
   services.AddScoped<ICRUDRepository<YourEntity>, DaprCRUDRepository<YourEntity>>();
   
   // Use in services
   public class YourService
   {
       private readonly ICRUDRepository<YourEntity> _repository;
       
       public YourService(ICRUDRepository<YourEntity> repository)
       {
           _repository = repository;
       }
   }
   ```

4. **Add Logging**
   ```csharp
   public class YourService
   {
       private readonly ILogger<YourService> _logger;
       
       public YourService(ILogger<YourService> logger)
       {
           _logger = logger;
       }
       
       public void YourMethod()
       {
           _logger.LogInformation("Processing started");
       }
   }
   ```

## Best Practices

1. **Domain Layer**
   - Keep domain models clean and free of infrastructure concerns
   - Use value objects for complex properties
   - Implement domain events for cross-cutting concerns

2. **Application Layer**
   - Use CQRS pattern for separating reads and writes
   - Implement validators for commands and queries
   - Use mediator pattern for handling commands

3. **Infrastructure Layer**
   - Implement repositories for each aggregate root
   - Use Unit of Work pattern for transaction management
   - Configure proper error handling and retries

4. **API Layer**
   - Use minimal APIs for lightweight endpoints
   - Implement proper validation and error handling
   - Use proper HTTP status codes and response formats

## Contributing

Please read our contributing guidelines before submitting pull requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
