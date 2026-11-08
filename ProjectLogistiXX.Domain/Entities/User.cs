using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Abstraction.Enums;
using ProjectLogistiXX.Domain.Common;
using ProjectLogistiXX.Domain.ValueObjects;
using ProjectLogistiXX.SharedKernel;

namespace ProjectLogistiXX.Domain.Entities
{
    public class User : BaseEntity
    {
        public UserId Id { get; private set; }
        public string Username { get; private set; } = string.Empty;

        public Email? Email { get; private set; }

        public PhoneNumber? PhoneNumber { get; private set; }

        public string Password { get; private set; } = string.Empty;
        public Address? Address { get; private set; }

        public UserRole Role { get; private set; }

        private User()
        {
            Id = null!;
        }

        private User(UserId id,
            string username,
            Email? email,
            PhoneNumber? phoneNumber,
            string password,
            Address? address,
            UserRole role)
        {
            Id = id;
            Username = username;
            Email = email;
            PhoneNumber = phoneNumber;
            Password = password;
            Address = address;
            Role = role;
        }

        public static Result<User> Create(
            string username,
            Email email,
            PhoneNumber phoneNumber,
            string password,
            Address? address,
            UserRole role

            )
        {

            if (string.IsNullOrEmpty(username))
            {
                return Error.Validation("UsernameInvalid", "Username-ul este obligatoriu.");
            }

            var userId = UserId.NewId();

            var user = new User(userId, username, email, phoneNumber, password, address, role);

            user.AddDomainEvent(new UserCreatedEvent(userId));

            return Result<User>.Success(user);

        }

        public void PromoteToManager()
        {
            if (Role == UserRole.Admin)
                throw new DomainException("Un admin nu poate fi retrogradat la manager.");

            Role = UserRole.Manager;
        }
    }
}
