using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Domain.Services;
using Dapr.Framework.Domain.Repositories;
using Dapr.Framework.Domain.Models;

namespace Dapr.Framework.Application.Services;

/// <summary>
/// Base implementation of generic service for CRUD operations
/// </summary>
/// <typeparam name="TEntity">Type of entity this service handles</typeparam>
/// <typeparam name="TRepository">Type of repository used for data access</typeparam>
public class ListDataService<TEntity, TRepository> : BaseDataService<TEntity, TRepository>, IListDataService<TEntity>
    where TEntity : class, IEntity
    where TRepository : class, IListRepository<TEntity>
{
    public ListDataService(TRepository repository) : base(repository)
    {

    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await Repository.GetAllAsync();
    }

    /// <inheritdoc />
    public virtual async Task<PagedList<TEntity>> GetPagedAsync(PaginationParameters parameters)
    {
        return await Repository.GetPagedAsync(parameters);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await Repository.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        return entity != null;
    }
}
