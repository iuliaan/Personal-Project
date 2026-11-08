using FluentValidation;
using FluentValidation.Results;

namespace ProjectLogistiXX.Test.TestApplication.ValidationDecorator
{
    internal class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    internal class InvalidCommandValidator : AbstractValidator<TestCommand>
    {
        public InvalidCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty()
                .WithMessage("Name is required")
                .WithErrorCode("NAME_REQUIRED");
        }
    }

    internal sealed class EmailValidator : AbstractValidator<TestCommandEmail>
    {
        public EmailValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .WithErrorCode("EMAIL_REQUIRED")
                .EmailAddress()
                .WithMessage("Email invalid")
                .WithErrorCode("EMAIL_INVALID");
        }
    }

    internal sealed class UsernameValidator : AbstractValidator<TestCommandEmail>
    {
        public UsernameValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required")
                .WithErrorCode("USERNAME_REQUIRED")
                .MinimumLength(3)
                .WithMessage("Too short")
                .WithErrorCode("USERNAME_TOO_SHORT");
        }
    }
}