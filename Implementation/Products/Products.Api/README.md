# Products API

This is a sample implementation of the Dapr Framework demonstrating a products management service using Clean Architecture and Dapr.

## Features

- Full CRUD operations for Product entities
- State persistence using Dapr and Redis
- OpenTelemetry integration for monitoring
- Swagger/OpenAPI documentation

## Project Structure

```
Products.Api/
├── Endpoints/           # API endpoint definitions
├── Services/           # Application services
├── dapr/              # Dapr configuration
│   ├── components/    # Dapr component configurations
│   └── config.yaml    # Dapr configuration file
└── Properties/        # Project properties and launch settings
```

## Configuration

The API uses the following ports by default:
- API: 5032
- Dapr HTTP: 3501
- Dapr gRPC: 50001

## Running the API

1. Make sure you have Dapr initialized:
   ```bash
   dapr init
   ```

2. Run the API with Dapr:
   ```bash
   ./run-debug.sh
   ```

## API Endpoints

### Products
- GET `/api/products`: Get all products
- GET `/api/products/{id}`: Get product by ID
- POST `/api/products`: Create a new product
- PUT `/api/products/{id}`: Update an existing product
- DELETE `/api/products/{id}`: Delete a product

### Product Model
```json
{
  "id": "string",
  "name": "string",
  "description": "string",
  "price": 0,
  "stockQuantity": 0,
  "isActive": true
}
```

## Development

### Prerequisites
- .NET Core 9.0 SDK
- Docker Desktop
- Dapr CLI

### Debug Configuration
The project includes VS Code launch configurations for:
- Products.Api with Dapr (Launch)
- Products.Api with Dapr (Attach)

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Development
- `ASPNETCORE_URLS`: http://localhost:5032
