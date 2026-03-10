using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Common;
using ProjectLogistiXX.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.ValueObjects
{
    public record Email
    {
        public string Value { get; init; }
        private const int MaxLength = 254;
        private const int MaxLocalPartLength = 64;
        private const int MinTldLength = 2;

        private Email() { }

        private Email(string value)
        {
            Value = value;
        }
        public static Result<Email> Create(string email)
        {

            if (string.IsNullOrWhiteSpace(email))
            {
                return Error.Validation("EmailInvalid", "Email-ul nu poate fi gol sau conține doar spații albe.");

            }
            email = email.Trim().ToLowerInvariant();
            var validateEmail = ValidateEmailFormat(email);

            if (validateEmail.IsFailure)
            {
                return validateEmail.Error;
            }

            return new Email(email);
        }
        public override string ToString() => Value;

        public static implicit operator string(Email email) => email.Value;

        private static Result ValidateEmailFormat(string email)
        {

            if (email.Length > MaxLength)
            {
                return Error.Validation("EmailTooLong", $"Email-ul este prea lung. Maxim {MaxLength} caractere.");
            }

            var structureResult = SplitAndValidateStructure(email);
            if (structureResult.IsFailure)
            {
                return structureResult.Error;
            }

            var (localPart, domainPart) = structureResult.Value;


            var localResult = ValidateLocalPart(localPart);

            if (localResult.IsFailure)
            {
                return localResult.Error;
            }
            var domainResult = ValidateDomainPart(domainPart);

            if (domainResult.IsFailure)
            {
                return domainResult.Error;
            }

            return Result.Success();

        }

        private static Result<(string localPart, string domainPart)> SplitAndValidateStructure(string email)
        {
            var atIndex = email.IndexOf('@');

            if (atIndex == -1)
            {
                return Error.Validation("EmailMissingAt", "Email-ul trebuie să conțină caracterul '@'.");
            }

            if (atIndex != email.LastIndexOf('@'))
            {
                return Error.Validation("EmailMultipleAt", "Email-ul nu poate conține mai multe caractere '@'.");
            }


            if (atIndex == 0 || atIndex == email.Length - 1)
            {
                return Error.Validation("EmailInvalidAtPosition", "Caracterul '@' nu poate fi la începutul sau sfârșitul email-ului.");
            }

            return (email[..atIndex], email[(atIndex + 1)..]);
        }

        private static Result ValidateDomainPart(string domainPart)
        {
            if (string.IsNullOrWhiteSpace(domainPart))
            {
                return Error.Validation("EmailInvalidDomain", "Partea de domeniu (după @) nu poate fi goală.");
            }

            if (!domainPart.Contains('.'))
            {
                return Error.Validation("EmailMissingDomainDot", "Lipseste punctul din domeniu.");
            }


            var lastDotIndex = domainPart.LastIndexOf('.');

            var tld = domainPart[(lastDotIndex + 1)..];

            if (tld.Length < MinTldLength)
            {
                return Error.Validation("EmailInvalidTld", $"Extensia domeniului trebuie să aibă cel puțin {MinTldLength} caractere.");
            }

            return ValidateParts(domainPart, "de domeniu", IsAllowedCharForDomainPart);
        }

        private static Result ValidateLocalPart(string localPart)
        {
            if (string.IsNullOrWhiteSpace(localPart))
            {
                return Error.Validation("EmailInvalidLocalPart", "Partea de utilizator (înainte de @) nu poate fi goală.");
            }

            if (localPart.Length > MaxLocalPartLength)
            {
                return Error.Validation("EmailLocalPartTooLong", $"Partea de utilizator este prea lungă. " +
                    $"Maxim {MaxLocalPartLength} caractere.");
            }

            return ValidateParts(localPart, "de utilizator", IsAllowedCharForLocalPart);

        }


        private static Result ValidateParts(string part, string partName, Func<char, bool> isAllowedChar)
        {
            if (part.Contains(".."))
            {
                return Error.Validation("EmailInvalidConsecutiveDots", "Email-ul nu poate conține două sau mai multe puncte consecutive.");
            }

            if (part.StartsWith('.') || part.EndsWith('.'))
            {
                return Error.Validation("EmailInvalidDotPosition", $"Email-ul nu poate începe sau termina cu un punct în partea {partName}.");
            }
            if (!char.IsLetterOrDigit(part[0]))
            {
                return Error.Validation("EmailInvalidStartCharacter", $"Email-ul nu poate începe cu caracterul '{partName[0]}'. " +
                    $"Trebuie să înceapă cu o literă sau cifră.");
            }

            if (!char.IsLetterOrDigit(part[^1]))
            {
                return Error.Validation("EmailInvalidEndCharacter", $"Email-ul nu se poate termina cu caracterul '{partName[^1]}'.");
            }

            foreach (char c in part)
            {
                if (!isAllowedChar(c))
                {
                    return Error.Validation("EmailInvalidCharacter", $"Caracter invalid '{c}' în partea {partName} a email-ului.");
                }
            }

            return Result.Success();

        }

        private static bool IsAllowedCharForLocalPart(char c)
        {
            return char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-' || c == '+';
        }

        private static bool IsAllowedCharForDomainPart(char c)
        {
            return char.IsLetterOrDigit(c) || c == '.' || c == '-';
        }
    }
}
