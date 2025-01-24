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
public class BaseDataService<TEntity, TRepository> : IBaseDataService<TEntity>
    where TEntity : class, IEntity
    where TRepository : class, IBaseRepository<TEntity>
{
    protected readonly TRepository Repository;

    public BaseDataService(TRepository repository)
    {
        Repository = repository;
    }
}
