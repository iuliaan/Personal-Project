using FluentAssertions;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using ProjectLogistiXX.Application.Abstraction.Users.Register;
using System.Net;
using System.Net.Http.Json;

namespace ProjectLogistiXX.Test.TestWebApi.TestRegisterUser
{
    public sealed class TestEndpointRegisterUser : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TestEndpointRegisterUser(TestWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RegisterUser_Should_Return_Created()
        {
            var request = new
            {
                username = "ionica_user",
                FirstName = "Ionica",
                LastName = "Pop",
                Email = "ionica.pop@example.com",
                PhoneNumber = "0753632081",
                Password = "SecurePassword123!",
                Address = new AddressDto(
                    "Strada Exemplu 123",
                    "Bucharest",
                    "Bucharest",
                    "Romania"
                ),
                Role = 1
            };

            var response = await _client.PostAsJsonAsync("/users/register", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            response.Headers.Location.Should().NotBeNull();
        }

        [Fact]
        public async Task RegisterUser_Should_Return_Conflict_When_Email_Already_Exists()
        {
            var request = RegisterRequestBuilder.Create().Build();

            var requestForAnotherUser = RegisterRequestBuilder.Create().Build();

            await _client.PostAsJsonAsync("/users/register", request);

            var response = await _client.PostAsJsonAsync("/users/register", requestForAnotherUser);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]

        public async Task RegisterUser_Should_Return_Invalidation_When_Username_Is_Invalid()
        {
            var request = RegisterRequestBuilder.Create()
                .WithUsername(string.Empty)
                .Build();

            var response = await _client.PostAsJsonAsync("/users/register", request);
            var content = await response.Content.ReadAsStringAsync();
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            content.Should().Contain("Username is required");
            problem.Should().NotBeNull();
            problem.Status.Should().Be(400);

        }

        [Fact]

        public async Task RegisterUser_Should_Return_Invalidation_When_PhoneNumber_Is_Invalid_And_Email_ExistedAlready()
        {
            var firstRequest = RegisterRequestBuilder.Create().Build();
            var secondRequest = RegisterRequestBuilder.Create()
                .WithPhoneNumber("+074536320aa81")
                .Build();

            await _client.PostAsJsonAsync("/users/register", firstRequest);

            var response = await _client.PostAsJsonAsync("/users/register", secondRequest);

            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            content.Should().Contain("Numarul de telefon nu poate contine litere sau simboluri");
        }

        private sealed record RegisterRequest(
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
