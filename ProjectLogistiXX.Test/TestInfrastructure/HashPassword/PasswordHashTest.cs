using Microsoft.Extensions.Options;
using ProjectLogistiXX.Infrastructure.Security;
using System.Security.Cryptography;
using Xunit.Abstractions;
using Xunit.Sdk;
using InfrastructurePasswordHash = ProjectLogistiXX.Infrastructure.Security.PasswordHash;

namespace ProjectLogistiXX.Test.TestInfrastructure.HashPassword
{
    public class PasswordHashTest
    {
        private readonly InfrastructurePasswordHash _passwordHash;
        private readonly ITestOutputHelper _output;

        public PasswordHashTest(ITestOutputHelper testOutput)
        {
            _output = testOutput;

            var testOptions = new PasswordHashOptions
            {
                Iterations = 1,
                Algorithm = HashAlgorithmName.SHA512
            };

            _passwordHash = new InfrastructurePasswordHash(Options.Create(testOptions));
        }

        [Fact]
        public void HashPassword_ShouldGenerateCorrectFormat()
        {
            var rawPassword = "ParolaMeaSuperSigur@123";
            var hashPassword = _passwordHash.HashPassword(rawPassword);

            var segments = hashPassword.Split('.');

            Assert.Equal(4, segments.Length);
            Assert.Equal("1", segments[0]);
            Assert.Equal("SHA256", segments[1]);
            Assert.False(string.IsNullOrEmpty(segments[2]));
            Assert.False(string.IsNullOrEmpty(segments[3]));
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsWrong()
        {
            var rawPassword = "ParolaMeaSuperSigur@123";
            var hashedPassword = _passwordHash.HashPassword(rawPassword);

            // Act
            var isValid = _passwordHash.VerifyPassword(hashedPassword, "AltaParolaGresita");

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void VerifyPassword_ShouldFail_WhenMinIterationsIsTooLow()
        {
            var rawPassword = "ParolaMeaSuperSigur@123";
            var hashedPassword = _passwordHash.HashPassword(rawPassword);

            // Act
            var isValid = _passwordHash.VerifyPassword(hashedPassword, rawPassword);

            // Assert
            Assert.False(isValid);
        }
    }
}
