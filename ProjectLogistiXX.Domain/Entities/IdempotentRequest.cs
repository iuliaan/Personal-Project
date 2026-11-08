using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Abstraction.Enums;
using ProjectLogistiXX.SharedKernel;

namespace ProjectLogistiXX.Domain.Entities
{
    public class IdempotentRequest
    {
        #region
        // Definim erorile statice aici, sus
        private static readonly Error NameNullOrEmpty =
            Error.Validation("IdempotentRequest.NameNullOrEmpty", "Name cannot be null or empty.");

        private static readonly Error ResponseNullOrEmpty =
            Error.Validation("IdempotentRequest.ResponseNullOrEmpty", "Response cannot be null or empty.");

        private static readonly Error NameTooLong =
            Error.Validation("IdempotentRequest.NameTooLong", $"Name cannot exceed {MaxNameLength} characters.");

        private static readonly Error AlreadyCompleted =
            Error.Validation("IdempotentRequest.AlreadyCompleted", "Request is already completed.");

        #endregion
        public IdempotencyKeyId Id { get; private set; }

        public UserId UserId { get; private set; } = null!;
        public Guid LockToken { get; private set; } = Guid.NewGuid();

        public string Name { get; private set; } = string.Empty;

        public string? Response { get; private set; }

        public IdempotencyStatus Status { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime? CompletedAt { get; private set; }

        public const int MaxNameLength = 200;

        private IdempotentRequest(IdempotencyKeyId id, UserId userId, string name, DateTime createdAtUtc)
        {
            Id = id;
            UserId = userId;
            Name = name;
            Response = null;
            Status = IdempotencyStatus.InProgress;
            CreatedAtUtc = createdAtUtc;
        }

        private IdempotentRequest()
        {
        }

        public static Result<IdempotentRequest> Reserve(
            IdempotencyKeyId id,
            UserId userId,
            string name,
            DateTime createdAtUtc)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return NameNullOrEmpty;
            }

            if (name.Length > MaxNameLength)
            {
                return NameTooLong;
            }

            var request = new IdempotentRequest(id, userId, name, createdAtUtc);

            return Result<IdempotentRequest>.Success(request);
        }

        public Result Complete(string response, DateTime completedAtUtc)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return ResponseNullOrEmpty;
            }

            if (Status == IdempotencyStatus.Completed)
            {
                return AlreadyCompleted;
            }

            if (Status != IdempotencyStatus.InProgress)
            {
                return Error.Validation("IdempotentRequest.InvalidStatus",
                    $"Cannot complete request with status {Status}");
            }

            Response = response;
            Status = IdempotencyStatus.Completed;
            CompletedAt = completedAtUtc;
            return Result.Success();
        }
    }
}
