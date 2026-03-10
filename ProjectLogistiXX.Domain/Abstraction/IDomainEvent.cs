using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.Abstraction
{
    public interface IDomainEvent
    {
        Guid Is { get; }

        DateTime OccurredOn { get; }
    }
}
