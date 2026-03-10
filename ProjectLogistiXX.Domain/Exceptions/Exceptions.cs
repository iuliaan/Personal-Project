using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.Entities
{
    public static partial class Exceptions
    {
        public static void AgainstNullOrEmpty(string value, string parameterName, string? message = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException(message ?? $"{parameterName} nu poate fi gol.");
            }
        }

        public static void AgainstInvalidEmail(string email, string parameterName)
        {
            AgainstNullOrEmpty(email, parameterName); // Refolosim logica
            email = email.Trim();


            if (!email.Contains('@'))
            {
                throw new DomainException($"Formatul email-ului pentru {parameterName} trebuie sa contina @");
            }
            else if (!email.Contains('.'))
            {
                throw new DomainException($"Formatul email-ului pentru {parameterName} trebuie sa contina .");
            }

            //_ = email switch
            //{
            //    _ when !email.Contains('@') => 
            //    throw new DomainException($"Formatul email-ului pentru {parameterName} trebuie sa contina @"),

            //    _ when !email.Contains('.') => 
            //    throw new DomainException($"Formatul email-ului pentru {parameterName} trebuie sa contina ."),
            //    _ => email // Cazul valid
            //};
        }

        public static void AgainstLength(string value, int minLength, string parameterName)
        {
            AgainstNullOrEmpty(value, parameterName);

            if (value.Length < minLength)
            {
                throw new DomainException($"{parameterName} trebuie să aibă minim {minLength} caractere.");
            }
        }

        public static void AgainstWeakPassword(string password, string parameterName)
        {
            AgainstNullOrEmpty(password, parameterName);
            CheckIfPasswordIsStrength(password, parameterName);
        }

        private static void CheckIfPasswordIsStrength(string password, string parameterName)
        {
            _ = password switch
            {
                _ when password.Length < 8 =>
                    throw new DomainException($"{parameterName} trebuie să aibă minim 8 caractere."),
                _ when !password.Any(char.IsUpper) =>
                    throw new DomainException($"{parameterName} trebuie să conțină cel puțin o literă mare."),
                _ when !password.Any(char.IsDigit) =>
                    throw new DomainException($"{parameterName} trebuie să conțină cel puțin o cifră."),
                _ when !password.Any(ch => !char.IsLetterOrDigit(ch)) =>
                    throw new DomainException($"{parameterName} trebuie să conțină cel puțin un caracter special."),
                _ => password // Cazul valid
            };
        }


    }
}
