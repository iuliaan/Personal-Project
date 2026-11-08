using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ProjectLogistiXX.Infrastructure.Persistence.Services
{
    public sealed class IdempotencyOptions
    {
        public const string SectionName = "Idempotency";

        public TimeSpan Ttl { get; init; } = TimeSpan.FromMinutes(10);

        public TimeSpan Retention { get; init; } = TimeSpan.FromHours(24);
        public TimeSpan CleanupInterval { get; init; } = TimeSpan.FromMinutes(15);

        public TimeSpan MaxCommandDuration { get; init; } = TimeSpan.FromMinutes(2);

    }
}
