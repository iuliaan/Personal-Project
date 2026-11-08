using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Presentation.Extensions;
using ProjectLogistiXX.SharedKernel;
using System.Security.Claims;

namespace ProjectLogistiXX.Presentation.Filters
{
    public sealed class IdempotencyFilter : IEndpointFilter
    {
        #region IdempotencyFilter Errors
        private static readonly Error EmptyGuid =
            Error.Validation("IdempotencyKey.EmptyGuid", "The Idempotency-Key header cannot be an empty GUID.");
        private static readonly Error InvalidFormat =
            Error.Validation("IdempotencyKey.InvalidFormat", "The Idempotency-Key header must be a valid GUID.");
        #endregion
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var requestContext = context.HttpContext.RequestServices.GetRequiredService<IRequestContext>();

            var idempotencyKey = context.HttpContext.Request.Headers["Idempotency-Key"].FirstOrDefault();

            if (string.IsNullOrEmpty(idempotencyKey))
            {
                return await next(context);
            }
            if (!Guid.TryParse(idempotencyKey, out var parsedGuid))
            {
                return InvalidFormat.ToProblem();
            }
            if (parsedGuid == Guid.Empty)
            {
                return EmptyGuid.ToProblem();
            }

            var userClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            requestContext.UserId = Guid.TryParse(userClaim, out var userId) ? new UserId(userId) : null;

            requestContext.IdempotencyKey = new IdempotencyKeyId(parsedGuid);

            return await next(context);
        }
    }
}
