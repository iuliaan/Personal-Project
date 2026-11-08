using Microsoft.EntityFrameworkCore;
using ProjectLogistiXX.Application.Abstraction.Data;
using ProjectLogistiXX.Application.Abstraction.Messaging;
using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Entities;
using ProjectLogistiXX.Domain.Interfaces;
using ProjectLogistiXX.Domain.ValueObjects;
using ProjectLogistiXX.SharedKernel;

namespace ProjectLogistiXX.Application.Abstraction.Users.Register
{
    internal class RegisterUserCommandHandler(IApplicationDbContext context, IPasswordHash passwordHash) :
        ICommandHandler<RegisterUserCommand, UserId>
    {

        public async Task<Result<UserId>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            return await CreateValueObject(command)
                           .BindAsync(vo => EnsureUserIsUnique(command, vo, cancellationToken))
                           .Bind(vo => CreateUserEntity(command, vo, passwordHash))
                           .BindAsync(user => SaveUserEntity(user, cancellationToken));
        }

        private static Result<RegistrationComponents> CreateValueObject(RegisterUserCommand command)
        {
            var emailResult = Email.Create(command.Email);
            var phoneResult = PhoneNumber.Create(command.PhoneNumber);
            var addressResult = Address.Create(
                command.Address.Street,
                streetNumber: null,
                buildingDetails: null,
                command.Address.City,
                command.Address.County,
                postalCode: null,
                command.Address.Country
            );

            var validationResults = Result.Combine(emailResult, phoneResult, addressResult);

            if (validationResults.IsFailure)
            {
                return Result<RegistrationComponents>.Failure(validationResults.Errors);
            }

            return Result<RegistrationComponents>.Success(new RegistrationComponents(
                emailResult.Value,
                phoneResult.Value,
                addressResult.Value));
        }

        private async Task<Result<RegistrationComponents>> EnsureUserIsUnique(
            RegisterUserCommand command,
            RegistrationComponents registrationComponents,
            CancellationToken cancellationToken)
        {
            var errorList = new List<Error>();

            if (await context.Users.AnyAsync(u => u.Email == registrationComponents.Email, cancellationToken))
            {
                errorList.Add(Error.AlreadyExists("User", "Email", registrationComponents.Email.Value));
            }

            if (await context.Users.AnyAsync(u => u.PhoneNumber == registrationComponents.PhoneNumber, cancellationToken))
            {
                errorList.Add(Error.AlreadyExists("User", "PhoneNumber", registrationComponents.PhoneNumber.Value));
            }

            if (await context.Users.AnyAsync(u => u.Username == command.Username, cancellationToken))
            {
                errorList.Add(Error.AlreadyExists("User", "Username", command.Username));
            }

            return errorList.Count > 0
                ? Result<RegistrationComponents>.Failure(errorList)
                : Result<RegistrationComponents>.Success(registrationComponents);
        }

        private static Result<User> CreateUserEntity(
                                            RegisterUserCommand command,
                                            RegistrationComponents registrationComponents,
                                            IPasswordHash passwordHash)
        {
            string hashedPassword = passwordHash.HashPassword(command.Password);

            var userResult = User.Create(
                command.Username,
                registrationComponents.Email,
                registrationComponents.PhoneNumber,
                hashedPassword,
                registrationComponents.Address,
                command.Role);


            return userResult;
        }

        private async Task<Result<UserId>> SaveUserEntity(User user, CancellationToken cancellationToken)
        {
            await context.Users.AddAsync(user, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result<UserId>.Success(user.Id);
        }
    }
}
