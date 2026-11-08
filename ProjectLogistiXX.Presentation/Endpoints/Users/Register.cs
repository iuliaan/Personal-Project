using ProjectLogistiXX.Application.Abstraction.Messaging;
using ProjectLogistiXX.Application.Abstraction.Users.Register;
using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Presentation.Extensions;
using ProjectLogistiXX.Presentation.Filters;

namespace ProjectLogistiXX.Presentation.Endpoints.Users
{
    public sealed class Register : IEndpoints
    {
        public void MapEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/users/register",
                async (Request request,
                ICommandHandler<RegisterUserCommand, UserId> handler,
                CancellationToken cancellationToken) =>
                {
                    var command = request.ToCommand();
                    var result = await handler.Handle(command, cancellationToken);

                    return result.IsFailure
                            ? result.Errors.ToProblem()
                            : Results.Created($"/users/{result.Value}", result.Value);
                })
                .WithTags("Users")
                .AddEndpointFilter<IdempotencyFilter>();
        }
    }
}
