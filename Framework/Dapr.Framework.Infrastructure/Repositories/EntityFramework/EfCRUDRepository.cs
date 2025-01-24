using Microsoft.EntityFrameworkCore;
using Dapr.Framework.Domain.Common;
using Dapr.Framework.Domain.Models;
using Dapr.Framework.Domain.Repositories;
using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Domain.Services;

namespace Dapr.Framework.Infrastructure.Repositories;

public class EfCRUDRepository<T> : EfListRepository<T>, ICRUDRepository<T> where T : class, IEntity
{

    private readonly ITransactionService _transactionService;

    public EfCRUDRepository(DbContext context, ITransactionService transactionService) : base(context)
    {
        _transactionService = transactionService;
    }

    private bool ShouldSaveChanges(bool saveChanges) => saveChanges && !_transactionService.HasActiveTransaction;

    public async virtual Task<T> CreateAsync(T entity, bool saveChanges = true)
    {
        await _dbSet.AddAsync(entity);
        if (ShouldSaveChanges(saveChanges))
        {
            await _context.SaveChangesAsync();
        }
        return entity;
    }

    public async virtual Task<T?> UpdateAsync(T entity, bool saveChanges = true)
    {
        _dbSet.Update(entity);
        if (ShouldSaveChanges(saveChanges))
        {
            await _context.SaveChangesAsync();
        }
        return entity;
    }

    public async virtual Task<bool> DeleteAsync(Guid id, bool saveChanges = true)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            if (ShouldSaveChanges(saveChanges))
            {
                await _context.SaveChangesAsync();
            }
            return true;
        }
        return false;
    }

    public async virtual Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
