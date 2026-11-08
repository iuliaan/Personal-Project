namespace ProjectLogistiXX.Application.Abstraction.Exceptions.Idempotency
{
    public class IdempotencyFatalException : Exception
    {
        public IdempotencyFatalException(string message) : base(message)
        {
        }
        public IdempotencyFatalException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
