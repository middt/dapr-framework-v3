using Dapr.Framework.Domain.Entities;

namespace Dapr.Framework.Domain.Common;

public abstract class BaseEntity : IEntity
{
    public Guid Id { get; set; }
}
