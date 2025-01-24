using System.Linq.Expressions;
using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Domain.Repositories;

namespace Workflow.Domain.Repositories;

public interface IBaseRepository<TEntity> : ICRUDRepository<TEntity> where TEntity : class, IEntity
{
    Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<bool> ExistsAsync(string id);
}