using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Domain.Common;
using Dapr.Framework.Domain.Models;
using Dapr.Framework.Domain.Repositories;
using Dapr.Framework.Domain.Entities;

namespace Dapr.Framework.Infrastructure.Repositories;

public class EfListRepository<T> : EfBaseRepository<T>, IListRepository<T> where T : class, IEntity
{

    public EfListRepository(DbContext context) : base(context)
    {

    }

    public async virtual Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async virtual Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async virtual Task<PagedList<T>> GetPagedAsync(PaginationParameters parameters)
    {
        var count = await _dbSet.CountAsync();
        var items = await _dbSet.Skip((parameters.PageNumber - 1) * parameters.PageSize)
                               .Take(parameters.PageSize)
                               .ToListAsync();

        return new PagedList<T>(items, count, parameters.PageNumber, parameters.PageSize);
    }
}
