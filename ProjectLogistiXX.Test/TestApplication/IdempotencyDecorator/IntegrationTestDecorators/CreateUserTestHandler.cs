using ProjectLogistiXX.Application.Abstraction.Messaging;
using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Abstraction.Enums;
using ProjectLogistiXX.Domain.Entities;
using ProjectLogistiXX.Domain.ValueObjects;
using ProjectLogistiXX.Infrastructure.Persistence;

namespace ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator.IntegrationTestDecorators
{
    public sealed record CreateUserTestCommand(string Email) : ICommand<TestResponse>;
    public sealed class CreateUserTestHandler(ApplicationDbContext dbContext)
        : ICommandHandler<CreateUserTestCommand, TestResponse>
    {
        public async Task<Result<TestResponse>> Handle(CreateUserTestCommand command, CancellationToken cancellationToken)
        {
            var userResult = User.Create(
                username: "testUser",
                email: Email.Create("testuser@example.com").Value,
                phoneNumber: PhoneNumber.Create("0744123456").Value,
                password: "test-hash",
                address: null,
                role: UserRole.Admin);

            dbContext.Users.Add(userResult.Value);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<TestResponse>.Success(new TestResponse(command.Email));
        }
    }
}
