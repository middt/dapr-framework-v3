using Dapr.Client;
using Dapr.Framework.Domain.Common;
using Dapr.Framework.Domain.Models;
using Dapr.Framework.Domain.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapr.Framework.Domain.Entities;

namespace Dapr.Framework.Infrastructure.Repositories;

public class DaprCRUDRepository<T> : DaprListRepository<T>, ICRUDRepository<T> where T : class, IEntity
{

    public DaprCRUDRepository(DaprClient daprClient) : base(daprClient)
    {

    }


    public async Task<T> CreateAsync(T entity, bool saveChanges = true)
    {
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        var stateKey = $"{_entityType}-{entity.Id}";
        await _daprClient.SaveStateAsync(_storeName, stateKey, entity);

        // Update keys list
        var keys = await _daprClient.GetStateAsync<List<string>>(_storeName, $"{_entityType}-keys") ?? new List<string>();
        if (!keys.Contains(stateKey))
        {
            keys.Add(stateKey);
            await _daprClient.SaveStateAsync(_storeName, $"{_entityType}-keys", keys);
        }

        return entity;
    }

    public async Task<T?> UpdateAsync(T entity, bool saveChanges = true)
    {
        var stateKey = $"{_entityType}-{entity.Id}";
        var existing = await GetByIdAsync(entity.Id);
        if (existing == null)
            return null;

        await _daprClient.SaveStateAsync(_storeName, stateKey, entity);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, bool saveChanges = true)
    {
        try
        {
            var stateKey = $"{_entityType}-{id}";
            var existing = await GetByIdAsync(id);
            if (existing == null)
                return false;

            // Update keys list
            var keys = await _daprClient.GetStateAsync<List<string>>(_storeName, $"{_entityType}-keys");
            if (keys != null && keys.Contains(stateKey))
            {
                keys.Remove(stateKey);
                await _daprClient.SaveStateAsync(_storeName, $"{_entityType}-keys", keys);
            }

            // Delete the entity
            await _daprClient.DeleteStateAsync(_storeName, stateKey);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        // For Dapr, changes are saved immediately, so this is a no-op
        return await Task.FromResult(0);
    }
}
