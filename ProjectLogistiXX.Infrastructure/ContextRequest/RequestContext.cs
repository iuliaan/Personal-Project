using ProjectLogistiXX.Domain.Abstraction;

namespace ProjectLogistiXX.Infrastructure.ContextRequest
{
    public sealed class RequestContext : IRequestContext
    {
        public IdempotencyKeyId? IdempotencyKey { get; set; }
        public UserId? UserId { get; set; }
    }
}
