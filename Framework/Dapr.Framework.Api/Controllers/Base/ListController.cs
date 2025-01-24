using Dapr.Framework.Application.Services;
using Dapr.Framework.Domain.Common;
using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Domain.Models;
using Dapr.Framework.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dapr.Framework.Api.Controllers.Base;

public abstract class ListController<TEntity, TService> : BaseController<TEntity, TService>
    where TEntity : class, IEntity
    where TService : class, IListDataService<TEntity>
{
    protected ListController(TService service) : base(service)
    {
    }

    [HttpGet]
    public virtual async Task<ActionResult<IEnumerable<TEntity>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("paged")]
    public virtual async Task<ActionResult<PagedList<TEntity>>> GetPaged([FromQuery] PaginationParameters parameters)
    {
        var pagedList = await _service.GetPagedAsync(parameters);
        return Ok(pagedList);
    }

    [HttpGet("{id}")]
    public virtual async Task<ActionResult<TEntity>> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }
}
