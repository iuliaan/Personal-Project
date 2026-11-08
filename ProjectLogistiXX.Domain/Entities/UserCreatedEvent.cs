using ProjectLogistiXX.Domain.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.Entities
{
    public sealed record UserCreatedEvent(UserId UserId) : IDomainEvent
    {
    }
}
