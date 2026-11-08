namespace ProjectLogistiXX.Application.Abstraction.Contracts.Idempotency
{
    public enum IdempotencyReservationStatus
    {
        Reserved,
        Completed,
        InProgress
    }
    public sealed class IdempotencyReservationResult<TResponse> where TResponse : class
    {
        public IdempotencyReservationStatus Status { get; private init; }
        public TResponse? Response { get; private init; }

        public Guid LockToken { get; private init; }

        public bool Reserved => Status == IdempotencyReservationStatus.Reserved;
        public bool Completed => Status == IdempotencyReservationStatus.Completed;
        public bool InProgress => Status == IdempotencyReservationStatus.InProgress;

        private IdempotencyReservationResult() { }

        public static IdempotencyReservationResult<TResponse> NewReservation(Guid lockToken) =>
            new()
            {
                Status = IdempotencyReservationStatus.Reserved,
                LockToken = lockToken,
                Response = null
            };

        public static IdempotencyReservationResult<TResponse> NewCompleted(TResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);

            return new()
            {
                Status = IdempotencyReservationStatus.Completed,
                Response = response
            };

        }

        public static IdempotencyReservationResult<TResponse> NewInProgress() =>
            new()
            {
                Status = IdempotencyReservationStatus.InProgress,
            };
    }
}
