using Aspire.Hosting;
using Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var statestore = builder.AddDaprComponent("statestore", "state.redis", new DaprComponentOptions
{
    LocalPath = "../components/statestore.yaml"
});

var lockstore = builder.AddDaprComponent("lockstore", "lock.redis", new DaprComponentOptions
{
    LocalPath = "../components/lockstore.yaml"
});

var externalService = builder.AddDaprComponent("httpbin", "service.http", new DaprComponentOptions
{
    LocalPath = "../components/external-service.yaml"
});


/*
var frameworkApi = builder.AddProject("framework-api", "../Framework/Dapr.Framework.Api")
    .WithDaprSidecar()
    .WithReference(statestore);
*/
var productsApi = builder.AddProject<Projects.Products_Api>("products-api")
    .WithDaprSidecar()
//    .WithReference(frameworkApi)
    .WithReference(statestore)
    .WithReference(lockstore)
    .WithReference(externalService);

builder.Build().Run();
