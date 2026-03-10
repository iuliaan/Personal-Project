using Microsoft.AspNetCore.Identity;
using Moq;
using ProjectLogistiXX.Domain.Abstraction.Enums;
using ProjectLogistiXX.Domain.Entities;
using ProjectLogistiXX.Domain.Interfaces;
using ProjectLogistiXX.Domain.ValueObject;
using ProjectLogistiXX.Domain.ValueObjects;

namespace ProjectLogistiXX.Test
{
    public class UnitTest1
    {
        //[Fact]
        //public void Create_ShouldReturnValidUser_WhenDataIsCorrect()
        //{
        //    var email = "test@example.com";
        //    var username = "testuser";
        //    var plainPassword = "ParolaBuna1!";
        //    var expectedHash = "hashed_security_token_123";
        //    var role = UserRole.Admin;


        //    var mockHasher = new Mock<IPasswordHash>();
        //    mockHasher.Setup(h => h.HashPassword(plainPassword)).Returns(expectedHash);


        //    var user = User.Create(username, email, plainPassword, role, mockHasher.Object);


        //    Assert.NotNull(user);
        //    Assert.Equal(username, user.Username);
        //    Assert.Equal(email, user.Email);


        //    Assert.Equal(expectedHash, user.PasswordHash);
        //    Assert.Equal(role, user.Role);


        //    Assert.Equal(0, user.Id);


        //}

        //[Theory]
        //[InlineData("")]
        //[InlineData("   ")]
        //public void Create_ShouldThrowArgumentException_WhenUsernameIsInvalid(string invalidUsername)
        //{
        //    var mockHasher = new Mock<IPasswordHash>();

        //    var exception = Assert.Throws<DomainException>(() =>
        //        User.Create(invalidUsername, "valid@email.com", "hash", UserRole.User, mockHasher.Object));

        //    Assert.Equal("Numele nu poate fi gol.", exception.Message);
        //}

        [Theory]
        [InlineData("test@example.com")]
        [InlineData("john.doe@company.ro")]
        [InlineData("jane_doe@test.co.uk")]
        [InlineData("user+tag@gmail.com")]
        [InlineData("123user@numbers.com")]
        [InlineData("a@b.ro")]
        [InlineData("user.name+tag@example.co.uk")]
        public void Create_ShouldReturnEmail_WhenAddressIsValid(string validEmail)
        {
            var email = Email.Create(validEmail);

            Assert.True(email.IsSuccess);
            Assert.NotNull(email.Value);
            Assert.Equal(validEmail, email.Value.Value);
        }

        [Theory]

        [InlineData("user@com")]
        [InlineData("user@examplecom")]
        [InlineData("user@.example.com")]
        [InlineData("user@example.com.")]
        [InlineData("user@example..com")]


        [InlineData("user@example.c")]

        [InlineData("test@@example.com")]
        [InlineData("user@exam_ple.com")]
        [InlineData("user@example .com")]
        [InlineData("user@example.com/")]
        public void Create_ShouldThrow_WhenDomainPartIsInvalid(string invalidEmail)
        {
            var result = Email.Create(invalidEmail);

            Assert.True(result.IsFailure);
        }

        [Theory]

        [InlineData("test@domeniu", "EmailMissingDomainDot")]
        [InlineData("test@domeniu.c", "EmailInvalidTld")]
        [InlineData("@domeniu.com", "EmailInvalidAtPosition")]
        [InlineData("test@", "EmailInvalidAtPosition")]


        [InlineData("test..test@domeniu.com", "EmailInvalidConsecutiveDots")]
        [InlineData("test@domeniu..com", "EmailInvalidConsecutiveDots")]
        [InlineData(".test@domeniu.com", "EmailInvalidDotPosition")]
        [InlineData("test.@domeniu.com", "EmailInvalidDotPosition")]


        [InlineData("-test@domeniu.com", "EmailInvalidStartCharacter")]


        [InlineData("te!st@domeniu.com", "EmailInvalidCharacter")]
        [InlineData("test@domeni!u.com", "EmailInvalidCharacter")]
        [InlineData("te st@domeniu.com", "EmailInvalidCharacter")]
        public void Create_WithInvalidFormat_ShouldReturnSpecificFailure(string invalidEmail, string expectedErrorCode)
        {
            var result = Email.Create(invalidEmail);
            Assert.True(result.IsFailure, $"Expected failure for '{invalidEmail}', dar validarea a trecut.");
            Assert.Equal(expectedErrorCode, result.Error.Code);
        }

        [Fact]

        public void Create_PhoneNumber_ShouldTrimWhitespace()
        {
            var rawPhoneNumber = "   +40 712 345 678   ";
            var expectedPhoneNumber = "+40712345678";
            var result = PhoneNumber.Create(rawPhoneNumber);

            // result is Result<PhoneNumber>, so check success and compare the inner PhoneNumber.Value (string)
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedPhoneNumber, result.Value.Value);
        }

        //[Theory]
        //[InlineData("scurt", "minim 10 caractere")]           // Prea scurta
        //[InlineData("numeliteremari1!", "literă mare")]      // Fara litera mare
        //[InlineData("NUAMCIFRE!", "cifră")]                   // Fara cifre
        //[InlineData("NuAmCaractereSpeciale1", "caracter special")] // Fara caractere speciale
        //public void Create_WithWeakPassword_ShouldThrowException(string weakPassword, string expectedMessagePart)
        //{

        //    var mockHasher = new Mock<IPasswordHash>();


        //    var exception = Assert.Throws<DomainException>(() =>
        //        User.Create("ion", "ion@test.com", weakPassword, UserRole.Manager, mockHasher.Object));
        //    Assert.Contains(expectedMessagePart, exception.Message);
        //}
    }
}
