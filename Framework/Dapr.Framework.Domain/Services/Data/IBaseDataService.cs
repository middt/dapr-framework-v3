using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Domain.Models;

namespace Dapr.Framework.Domain.Services;

/// <summary>
/// Generic service interface for basic CRUD operations
/// </summary>
/// <typeparam name="T">Type of entity this service handles</typeparam>
public interface IBaseDataService<T> where T : class, IEntity
{
   
}
