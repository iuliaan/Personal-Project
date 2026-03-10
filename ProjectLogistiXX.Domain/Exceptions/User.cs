using ProjectLogistiXX.Domain.Abstraction.Enums;
using ProjectLogistiXX.Domain.Common;
using ProjectLogistiXX.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;

        public UserRole Role { get; private set; }

        private User()
        {
        }

        //public User(string username, string email, string passwordHash, UserRole role)
        //{
        //    Username = username;
        //    Email = email;
        //    PasswordHash = passwordHash;
        //    Role = role;
        //}

        public static User Create(
            string username,
            string email,

            UserRole role
            )
        {

            Exceptions.AgainstNullOrEmpty(username, "Numele");
            Exceptions.AgainstInvalidEmail(email, "Emailul");

            return new User
            {
                Username = username,
                Email = email,
                Role = role
            };
        }

        public void PromoteToManager()
        {
            if (Role == UserRole.Admin)
                throw new DomainException("Un admin nu poate fi retrogradat la manager.");

            Role = UserRole.Manager;
        }
    }
}
