using ProjectLogistiXX.Application.Abstraction.Users.Register;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Test.TestWebApi.TestRegisterUser
{
    internal sealed class RegisterRequestBuilder
    {
        private string Username = "ionica_user";
        private readonly string FirstName = "Ionica";
        private readonly string LastName = "Pop";
        private string Email = "ionica.pop@example.com";
        private string PhoneNumber = "0753632081";
        private readonly string Password = "SecurePassword123!";
        private readonly AddressDto Address = new(
            "Strada Exemplu 123",
            "Bucharest",
            "Bucharest",
            "Romania"
        );
        private int Role = 1;


        public static RegisterRequestBuilder Create() => new();

        public RegisterRequestBuilder WithUsername(string username)
        {
            Username = username;
            return this;
        }
        public RegisterRequestBuilder WithEmail(string email) { Email = email; return this; }
        public RegisterRequestBuilder WithPhoneNumber(string phoneNumber) { PhoneNumber = phoneNumber; return this; }
        public RegisterRequestBuilder WithRole(int role) { Role = role; return this; }

        public RegisterRequest Build() => new(
            Username,
            FirstName,
            LastName,
            Email,
            PhoneNumber,
            Password,
            Address,
            Role
        );

        internal record RegisterRequest(
           string Username,
           string FirstName,
           string LastName,
           string Email,
           string PhoneNumber,
           string Password,
           AddressDto Address,
           int Role);
    }
}
