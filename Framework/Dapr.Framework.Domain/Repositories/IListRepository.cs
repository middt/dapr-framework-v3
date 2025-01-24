using Dapr.Framework.Domain.Common;
using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Domain.Models;

namespace Dapr.Framework.Domain.Repositories;

/// <summary>
/// Generic repository interface for data access operations
/// </summary>
/// <typeparam name="T">Type of entity this repository handles</typeparam>
public interface IListRepository<T> : IBaseRepository<T> where T : class, IEntity
{
    /// <summary>
    /// Gets an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(Guid id);

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
}
