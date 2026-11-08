using ProjectLogistiXX.Application.Abstraction.Users.Register;
using ProjectLogistiXX.Domain.Abstraction.Enums;

namespace ProjectLogistiXX.Presentation.Endpoints.Users
{
    public record AddressRequest(string Street, string City, string County, string Country);
    public record Request(
        string Username,
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        string Password,
        AddressRequest Address,
        UserRole UserRole);
    public static class RegisterMapping
    {
        public static RegisterUserCommand ToCommand(this Request request)
        {
            return new RegisterUserCommand(
                request.Username,
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.Password,
                request.Address.ToModel(),
                request.UserRole);
        }

        public static AddressDto ToModel(this AddressRequest request) =>
            new(request.Street, request.City, request.County, request.Country);
    }
}
