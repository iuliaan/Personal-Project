using ProjectLogistiXX.SharedKernel;

namespace ProjectLogistiXX.Domain.Abstraction
{
    public abstract class ResultBase
    {
        private readonly IReadOnlyList<Error> _errors;

        protected ResultBase()
        {
            IsSuccess = true;
            _errors = [];
        }
        
        protected ResultBase(Error error)
        {
            if (error is null)
            {
                throw new ArgumentNullException(nameof(error), "Eroarea nu poate fi null.");
            }

            IsSuccess = false;
            _errors = [error];
        }

        protected ResultBase(IReadOnlyList<Error> errors)
        {
            if (errors is null || errors.Count == 0)
            {
                throw new ArgumentException("Trebuie să oferi cel puțin o eroare pentru un Result eșuat.", nameof(errors));
            }

            IsSuccess = false;
            _errors = errors;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public Error Error => IsFailure
            ? _errors[0]
            : throw new InvalidOperationException("Cannot access the error of a successful result.");

        public IReadOnlyList<Error> Errors => IsFailure
            ? _errors
            : throw new InvalidOperationException("Cannot access the errors of a successful result.");

        public static Result Combine(params ResultBase[] results)
        {

            foreach (var result in results)
            {
                if (result.IsFailure)
                {
                    return Result.Failure(result.Errors);
                }
            }
            return Result.Success();
        }
    }

    public sealed class Result<T> : ResultBase
    {
        private readonly T? _value;

        private Result(T? value)
            : base()
        {
            _value = value;
        }

        private Result(Error error)
            : base(error)
        {
            _value = default;
        }

        private Result(IReadOnlyList<Error> errors)
            : base(errors)
        {
            _value = default;
        }

        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access the value of a failed result.");

        public static Result<T> Success(T value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null for a successful result.");
            }

            return new Result<T>(value);
        }

        public static Result<T> Failure(Error error)
            => new(error);

        public static Result<T> Failure(IReadOnlyList<Error> errors)
            => new(errors);

        //public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
        //    => IsSuccess ? onSuccess(_value!) : onFailure(Error);

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure)
            => IsSuccess ? onSuccess(_value!) : onFailure(Errors);

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(Error error) => Failure(error);
    }

    public sealed class Result : ResultBase
    {
        private Result()
            : base()
        {
        }

        private Result(Error error)
            : base(error)
        {
        }

        private Result(IReadOnlyList<Error> errors)
            : base(errors)
        {
        }

        public static Result Success()
            => new();

        public static Result Failure(Error error)
            => new(error);

        public static Result Failure(IReadOnlyList<Error> errors)
            => new(errors);


        public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
            => IsSuccess ? onSuccess() : onFailure(Error);

        public TResult Match<TResult>(Func<TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure)
            => IsSuccess ? onSuccess() : onFailure(Errors);

        public static implicit operator Result(Error error) => Failure(error);
    }
}
