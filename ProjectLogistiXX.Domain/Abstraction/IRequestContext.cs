namespace ProjectLogistiXX.Domain.Abstraction

{
    public interface IRequestContext
    {
        public IdempotencyKeyId? IdempotencyKey { get; set; }

        public UserId? UserId { get; set; }
    }
}
