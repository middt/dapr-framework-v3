#!/bin/bash
# Set environment variables
# export ASPNETCORE_ENVIRONMENT=Development
# export ASPNETCORE_URLS=http://localhost:5032

# Run with Dapr
echo "Starting Products.Api with Dapr..."
echo "You're up and running!" # Add this line to match the problemMatcher
dapr run \
    --app-id products-api \
    --app-port 5062 \
    --dapr-http-port 52010 \
    --dapr-grpc-port 52011 \
    --components-path ./components \
    # --config ./dapr/config.yaml \
    --enable-api-logging \
    --log-level debug \
    dotnet run
