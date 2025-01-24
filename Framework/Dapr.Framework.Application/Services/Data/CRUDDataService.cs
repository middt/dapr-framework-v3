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
public class CRUDDataService<TEntity, TRepository> : ListDataService<TEntity, TRepository>, ICRUDDataService<TEntity>
    where TEntity : class, IEntity
    where TRepository : class, ICRUDRepository<TEntity>
{
    public CRUDDataService(TRepository repository) : base(repository)
    {

    }


    /// <inheritdoc />
    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        return await Repository.CreateAsync(entity);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> UpdateAsync(Guid id, TEntity entity)
    {
        entity.Id = id;
        return await Repository.UpdateAsync(entity);
    }

    /// <inheritdoc />
    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        return await Repository.DeleteAsync(id);
    }

}
