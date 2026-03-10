using ProjectLogistiXX.Domain.Abstraction.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.Events
{
    public record UserCreatedDomainEvents(Guid UserId, string Username, string Email, string PasswordHash, UserRole Role) : IDomainEvent
    {
    }
}
