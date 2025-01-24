using Dapr.Client;
using Dapr.Framework.Domain.Common;
using Dapr.Framework.Domain.Models;
using Dapr.Framework.Domain.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapr.Framework.Domain.Entities;

namespace Dapr.Framework.Infrastructure.Repositories;

public class DaprListRepository<T> : DaprBaseRepository<T>, IListRepository<T> where T : class, IEntity
{

    public DaprListRepository(DaprClient daprClient) : base(daprClient)
    {

    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var stateKey = $"{_entityType}-{id}";
        return await _daprClient.GetStateAsync<T>(_storeName, stateKey);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            // Get all keys first
            var keys = await _daprClient.GetStateAsync<List<string>>(_storeName, $"{_entityType}-keys");
            if (keys == null || !keys.Any())
                return Enumerable.Empty<T>();

            // Get all items in bulk
            var states = await _daprClient.GetBulkStateAsync(_storeName, keys, parallelism: 1);

            var options = CreateJsonOptions();

            return states
                .Where(s => s.Value != null)
                .Select(s => JsonSerializer.Deserialize<T>(s.Value, options))
                .Where(item => item != null)!;
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error in GetAllAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<PagedList<T>> GetPagedAsync(PaginationParameters parameters)
    {
        var allItems = (await GetAllAsync()).ToList();
        var items = allItems
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();

        return new PagedList<T>(items, allItems.Count, parameters.PageNumber, parameters.PageSize);
    }

}
