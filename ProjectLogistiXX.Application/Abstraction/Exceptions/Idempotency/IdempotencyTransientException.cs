namespace ProjectLogistiXX.Application.Abstraction.Exceptions.Idempotency
{
    public class IdempotencyTransientException : Exception
    {
        public IdempotencyTransientException(string message) : base(message)
        {
        }
        public IdempotencyTransientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
