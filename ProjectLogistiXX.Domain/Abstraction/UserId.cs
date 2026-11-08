using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.Abstraction
{
    public sealed record UserId(Guid Value)
    {
        public static UserId NewId() => new(Guid.NewGuid());

    }

    public readonly record struct IdempotencyKeyId(Guid Value)
    {
        public static IdempotencyKeyId NewId() => new(Guid.NewGuid());
    }
}
