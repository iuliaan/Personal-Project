using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Common;
using ProjectLogistiXX.Domain.Entities;
using ProjectLogistiXX.Domain.ErrorsMessage;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.ValueObject
{
    public sealed record PhoneNumber
    {
        private const int MaxLenghtForRomania = 10; // Ex: 0744 123 456
        private const int MaxLengthForInternational = 15;
        private const string FirstPrefixForRomania = "+40";
        private const string SecondPrefixForRomania = "004";
        public string Value { get; }

        private PhoneNumber(string phoneNumber)
        {
            Value = phoneNumber;
        }

        public string ToDatabaseFormat()
        {
            return FormatPhoneNumberForDatabase(Value);
        }

        public static Result<PhoneNumber> Create(string numberPhone)
        {
            if (string.IsNullOrWhiteSpace(numberPhone))
            {
                return Result<PhoneNumber>.Failure(PhoneNumberErrors.NullOrEmpty);
            }

            var normalizedPhoneNumber = NormalizePhoneNumber(numberPhone);

            if (!normalizedPhoneNumber.IsSuccess)
            {
                return normalizedPhoneNumber.Error;
            }

            var validationResult = CleanPhoneNumber(normalizedPhoneNumber.Value);
            if (!validationResult.IsSuccess)
            {
                return normalizedPhoneNumber.Error;
            }

            return new PhoneNumber(validationResult.Value);

            //return Result<PhoneNumber>.Success(new PhoneNumber(FormatPhoneNumberForDisplay(normalizedPhoneNumber.Value)));
        }

        private static Result<string> NormalizePhoneNumber(string phoneNumber)
        {
            Span<char> buffer = stackalloc char[phoneNumber.Length];
            int index = 0;

            foreach (char c in phoneNumber)
            {
                if (Separators.Contains(c))
                {
                    continue;
                }

                if (char.IsLetter(c) || char.IsSymbol(c) || (c == '+' && index != 0))
                {
                    return Result<string>.Failure(Error.Validation("PhoneNumberInvalidCharacter",
                        "Numarul de telefon nu poate contine litere sau simboluri"));
                }

                buffer[index++] = c;
            }

            if (phoneNumber.StartsWith(FirstPrefixForRomania))
            {
                phoneNumber = phoneNumber[(FirstPrefixForRomania.Length - 1)..];
            }
            else if (phoneNumber.StartsWith(SecondPrefixForRomania))
            {
                phoneNumber = phoneNumber[(SecondPrefixForRomania.Length - 1)..];
            }

            return phoneNumber;

        }

        private static Result<string> CleanPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.Any(c => !char.IsDigit(c)))
            {
                return Result<string>.Failure(Error.Validation("PhoneNumberInvalid",
                    "Numarul de telefon nu poate contine litere."));
            }

            if (phoneNumber.Length > MaxLenghtForRomania)
            {
                return Result<string>.Failure(Error.Validation("PhoneNumberTooLong",
                    $"Numarul de telefon nu poate avea mai mult de {MaxLenghtForRomania} cifre."));
            }

            return Result<string>.Success(phoneNumber);

        }

        private static string FormatPhoneNumberForDatabase(string phoneNumber)
        {
            string dataBaseFormat = phoneNumber.Replace(" ", "");
            return FirstPrefixForRomania + dataBaseFormat[1..];
        }

        private static string FormatPhoneNumberForDisplay(string phoneNumber)
        {
            return $" {phoneNumber[..3]} {phoneNumber.Substring(3, 3)} {phoneNumber[6..]}";
        }

        private static readonly HashSet<char> Separators = [' ', '-', '.', '(', ')'];

        private static void ValidatePhoneNumberFormat(string phoneNumber)

        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new DomainException("Numărul de telefon nu poate fi gol.");
            }


            // Aici puteți adăuga logica de validare a formatului numărului de telefon
            // De exemplu, puteți verifica dacă numărul conține doar cifre și caractere permise
        }
    }
}
