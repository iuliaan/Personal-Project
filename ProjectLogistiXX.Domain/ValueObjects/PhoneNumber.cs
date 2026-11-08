using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.ExtensionMethod;
using ProjectLogistiXX.SharedKernel;

namespace ProjectLogistiXX.Domain.ValueObjects
{
    public sealed record PhoneNumber
    {
        private const int MaxLenghtForRomania = 10;
        private const string FirstPrefixForRomania = "+40";
        private const string SecondPrefixForRomania = "0040";
        private const int MaxLengthPhoneNumber = 25;
        private static readonly Error TooLong = Error.Validation("PhoneNumberTooLong", "Numarul de telefon este prea lung.");
        private static readonly Error TooShort = Error.Validation("PhoneNumberTooShort", "Numarul de telefon este prea scurt.");
        private static readonly Error NullOrEmpty = Error.Validation("PhoneNumberNullOrEmpty", "Numarul de telefon este obligatoriu");
        private static readonly Error InvalidCharacter = Error.Validation("PhoneNumberInvalidCharacter", 
            "Numarul de telefon nu poate contine litere sau simboluri");
        private static readonly HashSet<char> Separators = [' ', '-', '.', '(', ')'];
        public string Value { get; init; } = string.Empty;

        internal PhoneNumber(string value)
        {
            Value = value;
        }

        public string ToDatabaseFormat()
        {
            return FormatPhoneNumberForDatabase(Value);
        }

        public string DisplayFormat()
        {
            return FormatPhoneNumberForDisplay(Value);
        }

        public static Result<PhoneNumber> Create(string? numberPhone)
        {
            if (string.IsNullOrWhiteSpace(numberPhone))
            {
                return Result<PhoneNumber>.Failure(NullOrEmpty);
            }


            return ExtractValidateCharacters(numberPhone)
                .Map(NormalizeToLocalFormat)
                .Bind(CheckLenghtFormatForPhoneNumber)
                .Map(validatedNumber => new PhoneNumber(validatedNumber));

        }

        private static Result<string> ExtractValidateCharacters(string phoneNumber)
        {
            if (phoneNumber.AsSpan().IsDigitsOnly())
            {
                return phoneNumber;
            }

            if (phoneNumber.Length > MaxLengthPhoneNumber)
            {
                return Result<string>.Failure(TooLong);
            }

            return ParseAndCleanCharacters(phoneNumber);

        }

        private static Result<string> ParseAndCleanCharacters(string phoneNumber)
        {
            Span<char> buffer = stackalloc char[phoneNumber.Length];
            int index = 0;

            foreach (char c in phoneNumber)
            {
                if (Separators.Contains(c))
                {
                    continue;
                }

                if ((c == '+' && index == 0))
                {
                    buffer[index++] = '+';
                    continue;
                }

                if (!char.IsDigit(c))
                {
                    return Result<string>.Failure(InvalidCharacter);
                }

                buffer[index++] = c;
            }

            return new string(buffer[..index]);
        }

        private static string NormalizeToLocalFormat(string cleanedNumber)
        {
            if (cleanedNumber.StartsWith(FirstPrefixForRomania))
            {
                return "0" + cleanedNumber[FirstPrefixForRomania.Length..];
            }

            if (cleanedNumber.StartsWith(SecondPrefixForRomania))
            {
                return "0" + cleanedNumber[SecondPrefixForRomania.Length..];
            }


            return cleanedNumber;
        }

        private static Result<string> CheckLenghtFormatForPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.Length > MaxLenghtForRomania)
            {
                return Result<string>.Failure(TooLong);
            }

            if (phoneNumber.Length < MaxLenghtForRomania)
            {
                return Result<string>.Failure(TooShort);
            }

            return phoneNumber;
        }

        private static string FormatPhoneNumberForDatabase(string phoneNumber)
        {
            return FirstPrefixForRomania + phoneNumber[1..];
        }

        private static string FormatPhoneNumberForDisplay(string phoneNumber)
        {
            return $" {phoneNumber[..3]} {phoneNumber[3..6]} {phoneNumber[6..]}";
        }

    }
}
