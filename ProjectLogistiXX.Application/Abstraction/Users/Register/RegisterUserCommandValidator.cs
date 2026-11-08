using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Application.Abstraction.Users.Register
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.");

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Please specify a valid email address.");

            RuleFor(x => x.Username).NotEmpty().WithMessage("Please specify a username");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Please specify a password");
            RuleFor(x => x.Password)
                .MinimumLength(12)
                .WithMessage("Password must be at least 12 characters long")
                .MaximumLength(128)
                .WithMessage("Password must be at most 128 characters long");
        }
    }
}
