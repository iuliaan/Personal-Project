using FluentAssertions;
using ProjectLogistiXX.Domain.ValueObjects;

namespace ProjectLogistiXX.Test.TestDomain
{
    public class TestsPhoneNumberDomain
    {
        [Fact]

        public void Create_PhoneNumber_ShouldTrimWhitespace()
        {
            var rawPhoneNumber = "   +40 712 345 678   ";
            var expectedPhoneNumber = "+40712345678";
            var result = PhoneNumber.Create(rawPhoneNumber);
            var phoneNumberDatabaseFormat = result.Value?.ToDatabaseFormat();

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedPhoneNumber, phoneNumberDatabaseFormat);
        }

        [Theory]
        [InlineData("0744123456")]
        [InlineData("0721 234 567")]
        [InlineData("072-123-4567")]
        [InlineData("076.234.5678")]
        [InlineData("(072)1234567")]
        public void Create_WithValidRomanianLocalFormat_ReturnsSuccess(string input)
        {
            var result = PhoneNumber.Create(input);

            Assert.True(result.IsSuccess, $"Expected success for: {input}");
        }

        [Theory]
        [InlineData("0744123456")]
        [InlineData("+40744123456")]
        [InlineData("0040744123456")]
        [InlineData("0744 123 456")]
        [InlineData("+40 744 123 456")]
        [InlineData("0744-123-456")]
        [InlineData("0744.123.456")]
        [InlineData("(0744) 123 456")]
        [InlineData("+40 (744) 123-456")]
        public void Create_WhenPhoneNumberIsValid_ShouldReturnSuccess(string validNumber)
        {

            var result = PhoneNumber.Create(validNumber);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Value.Should().Be("0744123456");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WhenPhoneNumberIsNullOrWhitespace_ShouldReturnNullOrEmptyError(string? invalidNumber)
        {
            // Act
            var result = PhoneNumber.Create(invalidNumber);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [Theory]
        [InlineData("074412A456")]
        [InlineData("0744#12456")]
        [InlineData("abc")]
        [InlineData("074412345@")]
        [InlineData("074412345!")]
        public void Create_WithInvalidCharacters_ReturnsFailure(string input)
        {
            var result = PhoneNumber.Create(input);

            result.IsFailure.Should().BeTrue();
            result.Error.Message.Should().Be("Numarul de telefon nu poate contine litere sau simboluri");
            result.Error.Code.Should().Be("PhoneNumberInvalidCharacter");

        }

        [Fact]
        public void TwoPhoneNumbers_WithSameLocalNumber_AreEqual()
        {
            var a = PhoneNumber.Create("0744123456");
            var b = PhoneNumber.Create("0744123456");

            Assert.True(a.IsSuccess && b.IsSuccess);
            Assert.Equal(a.Value, b.Value);
        }

        [Fact]
        public void TwoPhoneNumbers_DifferentNumbers_AreNotEqual()
        {
            var a = PhoneNumber.Create("0744123456");
            var b = PhoneNumber.Create("0721987654");

            Assert.True(a.IsSuccess && b.IsSuccess);
            Assert.NotEqual(a.Value, b.Value);
        }

        [Theory]
        [InlineData("0744123456", " 074 412 3456")]
        [InlineData("+40744123456", " 074 412 3456")] // Pentru că intern a fost curățat tot la 0744123456
        public void ToDisplayFormat_WhenValidPhoneNumber_ShouldFormatCorrectly(string inputNumber, string expectedDisplayFormat)
        {

            var phoneNumberResult = PhoneNumber.Create(inputNumber);
            var phoneNumber = phoneNumberResult.Value;

            var displayFormat = phoneNumber.DisplayFormat();

            displayFormat.Should().Be(expectedDisplayFormat);
        }

        [Fact]
        public void Create_WithSingleDigit_ReturnsFailure()
        {
            var result = PhoneNumber.Create("7");

            Assert.False(result.IsSuccess);
        }
    }
}
