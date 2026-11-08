using ProjectLogistiXX.Domain.ValueObjects;

namespace ProjectLogistiXX.Application.Abstraction.Users.Register
{
    public sealed record RegistrationComponents(
        Email Email,
        PhoneNumber PhoneNumber,
        Address Address);
}
