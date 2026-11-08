using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.SharedKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.ValueObjects
{
    public sealed record PasswordHash
    {
        public string Value { get; }

        internal PasswordHash   (string value)
        {
            Value = value;
        }

        public static Result<PasswordHash> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result<PasswordHash>.Failure(Error.Validation("InvalidPasswordHash",
                    "Password cannot be empty or whitespace."));
            }

            return Result<PasswordHash>.Success(new PasswordHash(value));
        }
    }
}
