using Dapr.Framework.Domain.Common;
using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Domain.Models;

namespace Dapr.Framework.Domain.Repositories;

/// <summary>
/// Generic repository interface for data access operations
/// </summary>
/// <typeparam name="T">Type of entity this repository handles</typeparam>
public interface IBaseRepository<T> where T : class, IEntity
{
  
}
