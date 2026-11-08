using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.SharedKernel;

namespace ProjectLogistiXX.Domain.ValueObjects
{
    public record Email
    {
        #region Constants
        private const int MaxLength = 254;
        private const int MaxLocalPartLength = 64;
        private const int MinTldLength = 2;
        #endregion
        #region Errors
        private static readonly Error EmailNullOrEmpty = Error.Validation("EmailInvalid",
            "Email-ul nu poate fi gol sau conține doar spații albe.");
        private static readonly Error EmailTooLong = Error.Validation("EmailTooLong",
            "Email-ul este prea lung. Maxim 254 caractere.");
        private static readonly Error EmailMissingAt = Error.Validation("EmailMissingAt",
            "Email-ul trebuie să conțină caracterul '@'.");
        private static readonly Error EmailMultipleAt = Error.Validation("EmailMultipleAt",
            "Email-ul nu poate conține mai multe caractere '@'.");
        private static readonly Error EmailInvalidAtPosition = Error.Validation("EmailInvalidAtPosition",
            "Caracterul '@' nu poate fi la începutul sau sfârșitul email-ului.");
        private static readonly Error EmailInvalidDomain = Error.Validation("EmailInvalidDomain", "Partea de domeniu (după @) nu poate fi goală.");
        private static readonly Error EmailMissingDomainDot = Error.Validation("EmailMissingDomainDot",
            "Lipseste punctul din domeniu.");
        private static readonly Error EmailInvalidTld = Error.Validation("EmailInvalidTld",
            $"Extensia domeniului trebuie să aibă cel puțin {MinTldLength} caractere.");
        private static readonly Error EmailInvalidLocalPart = Error.Validation("EmailInvalidLocalPart",
            "Partea de utilizator (înainte de @) nu poate fi goală.");
        private static readonly Error EmailLocalPartTooLong = Error.Validation("EmailLocalPartTooLong",
            $"Partea de utilizator este prea lungă. Maxim {MaxLocalPartLength} caractere.");
        private static readonly Error EmailInvalidConsecutiveDots = Error.Validation("EmailInvalidConsecutiveDots",
            "Email-ul nu poate conține două sau mai multe puncte consecutive.");
        private static readonly Error EmailInvalidDotPosition = Error.Validation("EmailInvalidDotPosition",
            $"Email-ul nu poate începe sau termina cu un punct.");
        private static readonly Error EmailInvalidStartCharacter = Error.Validation("EmailInvalidStartCharacter",
            "Email-ul nu poate începe cu un caracter special. Trebuie să înceapă cu o literă sau cifră.");
        private static readonly Error EmailInvalidEndCharacter = Error.Validation("EmailInvalidEndCharacter",
            "Email-ul nu se poate termina cu un caracter special.");
        private static readonly Error EmailCharacterInvalid = Error.Validation("EmailInvalidCharacter",
            "Email-ul conține caractere nepermise.");
        #endregion 

        public string Value { get; init; } = string.Empty;
        //private Email() { }

        internal Email(string value)
        {
            Value = value;
        }
        public static Result<Email> Create(string email)
        {
            Result<string> emailResult = string.IsNullOrWhiteSpace(email) ? EmailNullOrEmpty : email;

            return emailResult
                .Map(e => e.Trim().ToLowerInvariant())
                .Ensure(ValidateEmailFormat)
                .Map(e => new Email(e));
        }
        public override string ToString() => Value;

        public static implicit operator string(Email email) => email.Value;

        private static Result ValidateEmailFormat(string email)
        {
            return email.ToProblem(e => e.Length > MaxLength, EmailTooLong)
                .Bind(SplitAndValidateStructure)
                .Ensure(tuple => ValidateLocalPart(tuple.localPart))
                .Ensure(tuple => ValidateDomainPart(tuple.domainPart))
                .Bind(_ => Result.Success());
        }

        private static Result<(string localPart, string domainPart)> SplitAndValidateStructure(string email)
        {
            //var atIndex = email.IndexOf('@');

            return email.IndexOf('@').ToProblem(index => index == -1, EmailMissingAt)
                .Ensure(index => index != email.LastIndexOf('@') ? EmailMultipleAt : Result.Success())
                .Ensure(index => index == 0 || index == email.Length - 1 ? EmailInvalidAtPosition : Result.Success())
                .Map(index => (localPart: email[..index], domainPart: email[(index + 1)..]));
        }

        private static Result ValidateDomainPart(string domainPart)
        {
            if (string.IsNullOrWhiteSpace(domainPart))
            {
                return EmailInvalidDomain;
            }

            if (!domainPart.Contains('.'))
            {
                return EmailMissingDomainDot;
            }


            var lastDotIndex = domainPart.LastIndexOf('.');

            var tld = domainPart[(lastDotIndex + 1)..];

            if (tld.Length < MinTldLength)
            {
                return EmailInvalidTld;
            }

            return ValidateParts(domainPart, "de domeniu", IsAllowedCharForDomainPart);
        }

        private static Result ValidateLocalPart(string localPart)
        {
            if (string.IsNullOrWhiteSpace(localPart))
            {
                return EmailInvalidLocalPart;
            }

            if (localPart.Length > MaxLocalPartLength)
            {
                return EmailLocalPartTooLong;
                //return Error.Validation("EmailLocalPartTooLong", $"Partea de utilizator este prea lungă. " +
                //    $"Maxim {MaxLocalPartLength} caractere.");
            }

            return ValidateParts(localPart, "de utilizator", IsAllowedCharForLocalPart);

        }
        private static Result ValidateParts(string part, string partName, Func<char, bool> isAllowedChar)
        {
            if (part.Contains(".."))
            {
                return EmailInvalidConsecutiveDots;
            }

            if (part.StartsWith('.') || part.EndsWith('.'))
            {
                return EmailInvalidDotPosition;
            }
            if (!char.IsLetterOrDigit(part[0]))
            {
                return EmailInvalidStartCharacter;
            }

            if (!char.IsLetterOrDigit(part[^1]))
            {
                return EmailInvalidEndCharacter;
            }

            foreach (char c in part)
            {
                if (!isAllowedChar(c))
                {
                    return EmailCharacterInvalid;
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
