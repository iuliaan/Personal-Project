using Microsoft.EntityFrameworkCore;
using ProjectLogistiXX.Application.Abstraction.Data;
using ProjectLogistiXX.Application.Abstraction.Users.Register;
using ProjectLogistiXX.Domain.Abstraction.Enums;
using ProjectLogistiXX.Domain.Entities;
using ProjectLogistiXX.Infrastructure.Persistence;
using ProjectLogistiXX.Domain.Interfaces;

namespace ProjectLogistiXX.Test.TestApplication.FakeTestRegisterUser
{
    public class RegisterUserCommandHandlerTest
    {
        private readonly TestApplicationDbContext _context;
        private readonly RegisterUserCommandHandler _handler;

        public RegisterUserCommandHandlerTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new TestApplicationDbContext(options);
            _handler = new RegisterUserCommandHandler(_context, new FakePasswordHash());

        }

        [Fact]
        public async Task Handle_ShouldRegisterUser()
        {
            var command = new RegisterUserCommand(
                Username: "ionica_user",
                FirstName: "Ionica",
                LastName: "Pop",
                Email: "ion@test.com",
                PhoneNumber: "0722123456",
                Password: "P@ssw0rd",
                Address: new AddressDto(
                    "Strada Florilor",
                    "Bucuresti",
                    "Ilfov",
                    "Romania"),
                Role: UserRole.Admin
            );

            var result = await _handler.Handle(command, default);
            Assert.True(result.IsSuccess);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "ionica_user");
            Assert.NotNull(user);

            Assert.Single(_context.Users);
            Assert.Equal("ionica_user", _context.Users.First().Username);

            Assert.Equal("ionica_user", user.Username);
            Assert.Equal("ion@test.com", user.Email?.Value);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_EmailAlreadyExists()
        {
            var command = new RegisterUserCommand(
               Username: "ionica_user",
               FirstName: "Ionica",
               LastName: "Pop",
               Email: "ion@test.com",
               PhoneNumber: "0722123456",
               Password: "P@ssw0rd",
               Address: new AddressDto(
                   "Strada Florilor",
                   "Bucuresti",
                   "Ilfov",
                   "Romania"
               ),
               Role: UserRole.User
           );


            await _handler.Handle(command, default);

            //var handlerWithFreshUow = new RegisterUserCommandHandler(_fakeRegisterUser, freshUnitOfWork);

            var duplicateEmailCommand = new RegisterUserCommand(
                Username: "ember_user",
                FirstName: "Ionica",
                LastName: "Pop",
                Email: "ion@test.com",
                PhoneNumber: "0722123456",
                Password: "P@ssw0rd",
                Address: new AddressDto(
                    "Strada Florilor",
                    "Bucuresti",
                    "Ilfov",
                    "Romania"
                ),
                Role: UserRole.User
               );

            var result2 = await _handler.Handle(duplicateEmailCommand, default);

            Assert.True(result2.IsFailure);
            Assert.Equal("EmailAlreadyExists", result2.Error.Code);

            Assert.Single(_context.Users);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_EmailIsInvalid()
        {
            var command = new RegisterUserCommand(
               Username: "ionica_user",
               FirstName: "Ionica",
               LastName: "Pop",
               Email: "invalid-email",
               PhoneNumber: "0722123456",
               Password: "P@ssw0rd",
               Address: new AddressDto(
                   "Strada Florilor",
                   "Bucuresti",
                   "Ilfov",
                   "Romania"
               ),
               Role: UserRole.User
           );

            var result = await _handler.Handle(command, default);

            Assert.True(result.IsFailure);
            Assert.Equal("EmailMissingAt", result.Error.Code);

            Assert.Empty(_context.Users);
            Assert.Equal(0, _context.SaveChangesCallCount);
        }
    }

    internal sealed class TestApplicationDbContext : ApplicationDbContext
    {
        public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
               : base(options)
        {
        }


        public int SaveChangesCallCount { get; private set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }

    public sealed class FakePasswordHash : IPasswordHash
    {
        public string HashPassword(string password) => $"hashed_{password}";

        public bool VerifyPassword(string hashedPassword, string providedPassword) => hashedPassword == $"hashed_{providedPassword}";
    }
}
