using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Domain.Common;
using Dapr.Framework.Domain.Models;
using Dapr.Framework.Domain.Repositories;
using Dapr.Framework.Domain.Entities;

namespace Dapr.Framework.Infrastructure.Repositories;

public class EfBaseRepository<T> : IBaseRepository<T> where T : class, IEntity
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public EfBaseRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

}
