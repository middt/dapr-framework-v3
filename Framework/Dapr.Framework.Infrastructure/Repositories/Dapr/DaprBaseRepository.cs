using Dapr.Client;
using Dapr.Framework.Domain.Common;
using Dapr.Framework.Domain.Models;
using Dapr.Framework.Domain.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapr.Framework.Domain.Entities;

namespace Dapr.Framework.Infrastructure.Repositories;

public class DaprBaseRepository<T> : IBaseRepository<T> where T : class, IEntity
{
    protected readonly DaprClient _daprClient;
    protected readonly string _storeName = "statestore";
    protected readonly string _entityType;

    public DaprBaseRepository(DaprClient daprClient)
    {
        _daprClient = daprClient;
        _entityType = typeof(T).Name.ToLowerInvariant();
    }

    protected static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}
