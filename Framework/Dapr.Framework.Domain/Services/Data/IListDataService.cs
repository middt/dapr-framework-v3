using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Domain.Models;

namespace Dapr.Framework.Domain.Services;

/// <summary>
/// Generic service interface for basic CRUD operations
/// </summary>
/// <typeparam name="T">Type of entity this service handles</typeparam>
public interface IListDataService<T> : IBaseDataService<T> where T : class, IEntity
{
    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <returns>Collection of all entities</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Gets a paged list of entities
    /// </summary>
    /// <param name="parameters">Pagination parameters</param>
    /// <returns>Paged list of entities</returns>
    Task<PagedList<T>> GetPagedAsync(PaginationParameters parameters);

    /// <summary>
    /// Gets an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(Guid id);

}
