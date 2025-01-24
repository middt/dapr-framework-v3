using Dapr.Framework.Domain.Common;
using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dapr.Framework.Api.Controllers.Base;

public abstract class BaseController<TEntity, TService> : ControllerBase
    where TEntity : class, IEntity
    where TService : class, IBaseDataService<TEntity>
{
    protected readonly TService _service;
    //protected readonly string _route;

    protected BaseController(TService service)
    {
        _service = service;
    }
}
