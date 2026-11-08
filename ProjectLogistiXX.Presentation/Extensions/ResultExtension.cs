using ProjectLogistiXX.Domain.Common;
using ProjectLogistiXX.SharedKernel;
using System.Collections.Frozen;

namespace ProjectLogistiXX.Presentation.Extensions
{
    public static class ResultExtension
    {
        private static readonly FrozenDictionary<ErrorType, int> StatusCodeMapping = new Dictionary<ErrorType, int>
        {
            { ErrorType.NotFound, 404 },
            { ErrorType.Validation, 400 },
            { ErrorType.Conflict, 409 },
            { ErrorType.Unauthorized, 401 },
            { ErrorType.Forbidden, 403 }

        }.ToFrozenDictionary();

        public static IResult ToProblem(this IEnumerable<Error> errors)
        {
            if (errors == null || !errors.Any())
            {
                return Results.Problem("An unexpected error occurred.");
            }

            if (errors.All(e => e.Type == ErrorType.Validation))
            {
                return CreateValidationProblem(errors);
            }

            var firstError = errors.First();

            int statusCode = StatusCodeMapping.TryGetValue(firstError.Type, out int code) ? code : 500;

            return Results.Problem(
                title: firstError.Code,
                detail: firstError.Message,
                //detail:errors.Select(e => e.Message)),
                statusCode: statusCode);
        }

        public static IResult ToProblem(this Error error)
        {
            return new[] { error }.ToProblem();
        }

        private static IResult CreateValidationProblem(IEnumerable<Error> errors)
        {
            var validationErrors = errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(e => e.Message)
                    .ToArray());
            
            return Results.ValidationProblem(validationErrors);
        }
    }
}
