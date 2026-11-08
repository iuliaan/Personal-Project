using ProjectLogistiXX.Domain.ValueObjects;

namespace ProjectLogistiXX.Test
{
    public class UnitTest1
    {

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
    }
}
