using Dapr.Framework.Application.Services;
using Dapr.Framework.Domain.Common;
using Dapr.Framework.Domain.Entities;
using Dapr.Framework.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dapr.Framework.Api.Controllers.Base;

public abstract class CRUDController<TEntity, TService> : ListController<TEntity, TService>
    where TEntity : class, IEntity
    where TService : class, ICRUDDataService<TEntity>
{
    protected CRUDController(TService service) : base(service)
    {
    }

    [HttpPost]
    public virtual async Task<ActionResult<TEntity>> Create([FromBody] TEntity entity)
    {
        var created = await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public virtual async Task<ActionResult<TEntity>> Update(Guid id, [FromBody] TEntity entity)
    {
        var updated = await _service.UpdateAsync(id, entity);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
