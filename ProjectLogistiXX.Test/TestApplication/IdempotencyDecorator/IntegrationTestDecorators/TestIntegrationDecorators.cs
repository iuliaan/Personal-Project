using ProjectLogistiXX.Application.Abstraction.Users.Register;
using ProjectLogistiXX.Domain.Abstraction.Enums;

namespace ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator.IntegrationTestDecorators
{
    public class TestIntegrationDecorators
    {
        public static RegisterUserCommand CreateValidCommand()
        {
            return new RegisterUserCommand(
                Username: "testuser",
                FirstName: "Test",
                LastName: "User",
                Email: "testuser@example.com",
                PhoneNumber: "0753632081",
                Password: "TestPassword123!",
                Address: new AddressDto(
                    "strada florilor",
                    "Bucuresti",
                    "Ilfov",
                    "Romania"
                ),
                Role: UserRole.User


            );
        }
    }
}
