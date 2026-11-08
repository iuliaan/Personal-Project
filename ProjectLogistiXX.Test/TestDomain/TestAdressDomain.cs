using FluentAssertions;
using ProjectLogistiXX.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Test.TestDomain
{
    public class TestAdressDomain
    {
        [Fact]
        public void Create_WithValidParameters_ShouldReturnSuccess()
        {

            string street = "Mihai Viteazul";
            string streetNumber = "14A";
            string buildingDetails = "Bl. 2, Ap. 5";
            string city = "Cluj-Napoca";
            string county = "Cluj";
            string postalCode = "400000";
            string country = "România";


            var result = Address.Create(street, streetNumber, buildingDetails, city, county, postalCode, country);


            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(street, result.Value.Street);
            Assert.Equal(streetNumber, result.Value.StreetNumber);
            Assert.Equal(buildingDetails, result.Value.BuildingDetails);
            Assert.Equal(city, result.Value.City);
            Assert.Equal(county, result.Value.County);
            Assert.Equal(postalCode, result.Value.PostalCode);
            Assert.Equal(country, result.Value.Country);


            Assert.Equal("Mihai Viteazul 14A, Cluj-Napoca, România", result.Value.DisplayValue);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithInvalidStreet_ShouldReturnFailure(string? invalidStreet)
        {

            var result = Address.Create(invalidStreet, "10", "Detalii", "Bucuresti", "Bucuresti", "010101", "Romania");

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("InvalidStreet");
            result.Error.Message.Should().Be("Street is required.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithInvalidStreetNumber_ShouldReturnFailure(string? invalidStreetNumber)
        {
            var result = Address.Create("Strada Mea", invalidStreetNumber, "Detalii", "Bucuresti", "Bucuresti", "010101", "Romania");

            Assert.False(result.IsSuccess);
            Assert.Equal("InvalidStreetNumber", result.Error.Code);
            Assert.Equal("Street number is required.", result.Error.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithInvalidCity_ShouldReturnFailure(string? invalidCity)
        {

            var result = Address.Create("Strada Mea", "10", "Detalii", invalidCity, "Bucuresti", "010101", "Romania");

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("InvalidCity");
            result.Error.Message.Should().Be("City is required.");
        }

        [Fact]
        public void Create_WithAllRequiredFieldsInvalid_ReturnsAllThreeErrors()
        {
            var result = Address.Create(null, null, "Ap.1", null, "Cluj", "400001", "Romania");

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().HaveCount(3);
            result.Errors.Should().Contain(e => e.Code == "InvalidStreet");
            result.Errors.Should().Contain(e => e.Code == "InvalidStreetNumber");
            result.Errors.Should().Contain(e => e.Code == "InvalidCity");
        }

        [Fact]
        public void Create_WithValidData_TrimsWhitespace()
        {
            var result = Address.Create(
                street: "  Str. Mihai Eminescu  ",
                streetNumber: "  12  ",
                buildingDetails: "  Ap. 3  ",
                city: "  Cluj-Napoca  ",
                county: "  Cluj  ",
                postalCode: "  400001  ",
                country: "  Romania  ");


            result.IsSuccess.Should().BeTrue();
            result.Value.Street.Should().Be("Str. Mihai Eminescu");
            result.Value.StreetNumber.Should().Be("12");
            result.Value.BuildingDetails.Should().Be("Ap. 3");
            result.Value.City.Should().Be("Cluj-Napoca");
            result.Value.County.Should().Be("Cluj");
            result.Value.PostalCode.Should().Be("400001");
            result.Value.Country.Should().Be("Romania");

        }

        [Fact]
        public void Create_WithNullOptionalFields_ReturnsSuccess()
        {
            var result = Address.Create(
                street: "Str. Libertatii",
                streetNumber: "5",
                buildingDetails: null,   // optional
                city: "Oradea",
                county: null,   // optional
                postalCode: null,   // optional
                country: "Romania");

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void TwoAddresses_WithDifferentCity_AreNotEqual()
        {
            var a1 = Address.Create(
                street: "Str. Libertatii",
                streetNumber: "5",
                buildingDetails: null,   // optional
                city: "Oradea",
                county: null,   // optional
                postalCode: null,   // optional
                country: "Romania").Value;

            var a2 = Address.Create(
                street: "Str. Libertatii",
                streetNumber: "5",
                buildingDetails: null,   // optional
                city: "Oradea-Mica",
                county: null,   // optional
                postalCode: null,   // optional
                country: "Romania").Value;

            Assert.NotEqual(a1, a2);
        }
    }
}
