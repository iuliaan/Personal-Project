using ProjectLogistiXX.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.Abstraction
{
    public abstract class ResultBase
    {
        private readonly Error _error;

        protected ResultBase(bool isSuccess, Error error)
        {
            if (!isSuccess && error is null)
                throw new ArgumentNullException(nameof(error), "Error cannot be null for a failed result.");

            IsSuccess = isSuccess;
            _error = error ?? Error.None;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public Error Error => IsFailure
            ? _error
            : throw new InvalidOperationException("Cannot access the error of a successful result.");
    }

    public sealed class Result<T> : ResultBase
    {
        private readonly T? _value;

        private Result(T? value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access the value of a failed result.");

        public static Result<T> Success(T value)
        {
            if (EqualityComparer<T>.Default.Equals(value, default))
                throw new ArgumentNullException(nameof(value), "Success value cannot be null.");

            return new Result<T>(value, true, Error.None);
        }

        public static Result<T> Failure(Error error)
            => new(default, false, error);

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
            => IsSuccess ? onSuccess(_value!) : onFailure(Error);

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(Error error) => Failure(error);
    }

    public sealed class Result : ResultBase
    {
        private Result(bool isSuccess, Error error)
            : base(isSuccess, error)
        {
        }

        public static Result Success()
            => new(true, Error.None);

        public static Result Failure(Error error)
            => new(false, error);

        public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
            => IsSuccess ? onSuccess() : onFailure(Error);

        public static implicit operator Result(Error error) => Failure(error);
    }
}
