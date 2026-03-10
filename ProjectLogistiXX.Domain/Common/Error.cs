using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.Common
{
    public sealed record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error NullValue = new("Error.NullValue", "Value cannot be null");

        public static Error NotFound(string entityName, Guid id)
        {
            return new Error($"{entityName}.NotFound", $"{entityName} with id {id} was not found");
        }

        public static Error Conflict(string code, string message) => new(code, message);

        public static Error Validation(string code, string message) => new(code, message);

        public static Error Unauthorized(string message = "Acces neautorizat") => new("Unauthorized", message);
    }
}
