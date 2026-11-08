namespace ProjectLogistiXX.SharedKernel
{
    public sealed record Error (string Code, string Message, ErrorType Type)
    {
        public static Error AlreadyExists(string entity, string property, string value) =>
            new($"{entity}.{property}AlreadyExists",
                $"Un utilizator cu {property} '{value}' deja exista.",
                ErrorType.Conflict);

        public static Error NotFound(string entity, string property, string value) =>
            new($"{entity}.{property}NotFound",
                $"Utilizator cu {property} '{value}' nu a fost gasit",
                ErrorType.NotFound);

        public static Error Validation(string code, string message) =>
            new(code, message, ErrorType.Validation);
    }
}
