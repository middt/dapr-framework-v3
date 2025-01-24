using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Domain.Models;

namespace Dapr.Framework.Domain.Services;

/// <summary>
/// Generic service interface for basic CRUD operations
/// </summary>
/// <typeparam name="T">Type of entity this service handles</typeparam>
public interface ICRUDDataService<T> : IListDataService<T> where T : class, IEntity
{
    /// <summary>
    /// Creates a new entity
    /// </summary>
    /// <param name="entity">The entity to create</param>
    /// <returns>The created entity</returns>
    Task<T> CreateAsync(T entity);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="entity">The updated entity data</param>
    /// <returns>The updated entity if found and updated, null otherwise</returns>
    Task<T?> UpdateAsync(Guid id, T entity);

    /// <summary>
    /// Deletes an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <returns>True if the entity was deleted, false if it wasn't found</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Checks if an entity exists
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <returns>True if the entity exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
}
