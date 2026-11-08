using FluentValidation;
using FluentValidation.Results;
using Microsoft.IdentityModel.Tokens.Experimental;
using ProjectLogistiXX.Application.Abstraction.Messaging;
using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Common;
using ProjectLogistiXX.SharedKernel;

namespace ProjectLogistiXX.Application.Abstraction.Decorators
{
    public sealed class ValidationDecorator<TCommand, TResponse> :
        ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly ICommandHandler<TCommand, TResponse> _handler;
        private readonly IEnumerable<IValidator<TCommand>> _validators;
        public ValidationDecorator(ICommandHandler<TCommand, TResponse> handler,
            IEnumerable<IValidator<TCommand>> validators)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        }

        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {

            ValidationFailure[] validationFailures = await ValidateAsync(command, cancellationToken);


            if (validationFailures.Length == 0)
            {
                return await _handler.Handle(command, cancellationToken);
            }

            return ValidationFailure(validationFailures);
        }

        private static Result<TResponse> ValidationFailure(IEnumerable<ValidationFailure> failures) =>
       Result<TResponse>.Failure([.. failures.Select(e => Error.Validation(e.ErrorCode, e.ErrorMessage))]);

        private async Task<ValidationFailure[]> ValidateAsync(TCommand command, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return [];
            }

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(command, cancellationToken))
            );

            var validationFailures = validationResults
               .SelectMany(r => r.Errors)
               .ToList();


            return [.. validationFailures];
        }    
    }
}
