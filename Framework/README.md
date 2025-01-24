# Dapr Framework

## Overview
The Dapr Framework provides a robust foundation for building distributed applications using Dapr building blocks. It offers standardized patterns for common microservices operations.

## Key Features
- **Distributed Cache**: Unified caching interface with Dapr state store and Redis implementations
- **External Services**: Standardized external service integration patterns
- **Data Services**: CRUD and list operations with multiple repository implementations
- **Distributed Locking**: Cross-service synchronization using Dapr and Redis
- **Telemetry**: Built-in observability and logging capabilities

## Architecture
The framework follows a layered architecture:
1. **API Layer**: REST endpoints and controllers
2. **Application Layer**: Business logic and service implementations
3. **Domain Layer**: Core entities and interfaces
4. **Infrastructure Layer**: Repository implementations and infrastructure services

## Getting Started
1. Add framework packages to your project
2. Configure services in Program.cs
3. Implement your business logic in the Application layer
4. Use provided base controllers for common operations

