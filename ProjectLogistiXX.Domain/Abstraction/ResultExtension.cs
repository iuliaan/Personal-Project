using ProjectLogistiXX.SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.Abstraction
{
    public static class ResultExtension
    {

        public static Result<T> ToProblem<T>(this T value, Func<T, bool> predicate, Error error)
        {
           return predicate(value) ? error : value;
        }
        public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapFunc)
        {
            if (result.IsSuccess)
            {
                return mapFunc(result.Value);
            }

            return result.Error;
        }

        public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> bindFunc)
        {
            if (result.IsSuccess)
            {
                return bindFunc(result.Value);
            }

            return result.Error;
        }

        public static Result Bind<TIn>(this Result<TIn> result, Func<TIn, Result> bindFunc)
        {
            if (result.IsSuccess)
            {
                return bindFunc(result.Value);
            }

            return Result.Failure(result.Error);
        }

        public static Result<TIn> Ensure<TIn>(this Result<TIn> result, Func<TIn, Result> validate)
        {
            if (result.IsFailure)
            {
                return result.Error;
            }

            var validationResult = validate(result.Value);

            return validationResult.IsSuccess
                ? result
                : validationResult.Error;
        }

        public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> mapFunc)
        {
            return result.IsSuccess
                           ? await mapFunc(result.Value)
                           : result.Error;
        }

        public static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> bindFunc)
        {
            return result.IsSuccess
                           ? await bindFunc(result.Value)
                           : result.Error;
        }

        public static async Task<Result> BindAsync<TIn>(this Result<TIn> result, Func<TIn, Task<Result>> bindFunc) =>
            result.IsSuccess
                           ? await bindFunc(result.Value)
                           : Result.Failure(result.Error);

        public static async Task<Result> BindAsync<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result>> bindFunc)
        {
            var result = await resultTask;
            return result.IsSuccess
                           ? await bindFunc(result.Value)
                           : Result.Failure(result.Error);
        }

        public static async Task<Result<TOut>> Bind<TIn, TOut>(
            this Task<Result<TIn>> resultTask,
            Func<TIn, Result<TOut>> bindFunc)
        {
            var result = await resultTask;
            return result.IsSuccess
                           ? bindFunc(result.Value)
                           : result.Error;
        }

        public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
            this Task<Result<TIn>> resultTask,
            Func<TIn, Task<Result<TOut>>> bindFunc)
        {
            var result = await resultTask;

            return result.IsSuccess
                           ? await bindFunc(result.Value)
                           : result.Error;
        }


    }
}
