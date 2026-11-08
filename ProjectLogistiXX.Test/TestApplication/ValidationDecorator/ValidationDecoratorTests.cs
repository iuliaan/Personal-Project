using FluentValidation;
using ProjectLogistiXX.Application.Abstraction.Decorators;
using ProjectLogistiXX.Application.Abstraction.Messaging;
using ProjectLogistiXX.Domain.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Test.TestApplication.ValidationDecorator
{
    public class ValidationDecoratorTests
    {
        [Fact]
        public async Task Handle_WhenNoValidators_ShouldCallInnerHandler()
        {
            var handler = new FakeHandler<TestCommand>();
            var validators = Enumerable.Empty<IValidator<TestCommand>>();

            var decorator = new ValidationDecorator<TestCommand, string>(handler, validators);
            var testCommand = new TestCommand("Test");

            var result = await decorator.Handle(testCommand, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(handler.WasCalled);
        }

        [Fact]
        public async Task Handle_Should_Return_Failure_When_Validation_Fails()
        {
            var handler = new FakeHandler<TestCommand>();
            var validators = new List<IValidator<TestCommand>>()
            {
                new InvalidCommandValidator()
            };

            var decorator = new ValidationDecorator<TestCommand, string>(handler, validators);

            var command = new TestCommand("");

            var result = await decorator.Handle(command, CancellationToken.None);


            Assert.False(result.IsSuccess);
            Assert.False(handler.WasCalled);
            Assert.NotEmpty(result.Errors);
        }

        [Fact]
        public async Task Handle_Should_Return_All_Errors_From_Multiple_Validators()
        {
            var handler = new FakeHandler<TestCommandEmail>();

            var validators = new List<IValidator<TestCommandEmail>>()
            {
                new EmailValidator(),
                new UsernameValidator()
            };

            var decorator = new ValidationDecorator<TestCommandEmail, string>(handler, validators);

            var command = new TestCommandEmail(
                Email: "",
                Username: "");

            var result = await decorator.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(handler.WasCalled);
            Assert.NotEmpty(result.Errors);

            Assert.Equal(4, result.Errors.Count);

            Assert.Contains(result.Errors, e => e.Code == "EMAIL_REQUIRED");
            Assert.Contains(result.Errors, e => e.Code == "EMAIL_INVALID");
            Assert.Contains(result.Errors, e => e.Code == "USERNAME_REQUIRED");
            Assert.Contains(result.Errors, e => e.Code == "USERNAME_TOO_SHORT");

        }
    }

    public sealed record TestCommand(string Name) : ICommand<string>;

    public sealed record TestCommandEmail(string Email, string Username) : ICommand<string>;
    public sealed class FakeHandler<TCommand> : ICommandHandler<TCommand, string> where TCommand : ICommand<string>
    {
        public bool WasCalled { get; private set; }

        public Task<Result<string>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult(Result<string>.Success("OK"));
        }
    }
}
