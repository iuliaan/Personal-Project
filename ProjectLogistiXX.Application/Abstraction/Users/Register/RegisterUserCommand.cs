using ProjectLogistiXX.Application.Abstraction.Messaging;
using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Abstraction.Enums;

namespace ProjectLogistiXX.Application.Abstraction.Users.Register
{
    public record AddressDto(
        string Street,
        string City,
        string County,
        string Country);
    public sealed record RegisterUserCommand(
        string Username,
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        string Password,
        AddressDto Address,
        UserRole Role) : ICommand<UserId>;
}
