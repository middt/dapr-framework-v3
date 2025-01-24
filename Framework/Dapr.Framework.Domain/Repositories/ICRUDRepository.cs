using Dapr.Framework.Domain.Common;
using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Domain.Models;

namespace Dapr.Framework.Domain.Repositories;

/// <summary>
/// Generic repository interface for data access operations
/// </summary>
/// <typeparam name="T">Type of entity this repository handles</typeparam>
public interface ICRUDRepository<T> : IListRepository<T> where T : class, IEntity
{
    /// <summary>
    /// Creates a new entity
    /// </summary>
    /// <param name="entity">The entity to create</param>
    /// <param name="saveChanges">Whether to save changes immediately</param>
    /// <returns>The created entity</returns>
    Task<T> CreateAsync(T entity, bool saveChanges = true);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="saveChanges">Whether to save changes immediately</param>
    /// <returns>The updated entity</returns>
    Task<T?> UpdateAsync(T entity, bool saveChanges = true);

    /// <summary>
    /// Deletes an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="saveChanges">Whether to save changes immediately</param>
    /// <returns>True if the entity was deleted, false if it wasn't found</returns>
    Task<bool> DeleteAsync(Guid id, bool saveChanges = true);

    /// <summary>
    /// Saves all pending changes in the repository
    /// </summary>
    /// <returns>Number of affected records</returns>
    Task<int> SaveChangesAsync();
}
