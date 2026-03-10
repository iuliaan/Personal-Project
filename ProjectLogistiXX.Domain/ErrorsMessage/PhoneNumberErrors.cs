using ProjectLogistiXX.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.ErrorsMessage
{
    public static class PhoneNumberErrors
    {
        public static readonly Error NullOrEmpty = new
            ("PhoneNumber.NullOrEmpty", 
            "Phone number cannot be null or empty.");

        public static readonly Error InvalidFormat = new(
            "PhoneNumber.InvalidFormat",
            "Phone number contains invalid characters. Only digits, +, spaces, -, (, ) are allowed.");

        public static readonly Error TooShort = new(
            "PhoneNumber.TooShort",
            "Phone number is too short. Minimum 10 digits required.");


        public static readonly Error TooLong = new(
            "PhoneNumber.TooLong",
            "Phone number is too long. Maximum 15 digits allowed.");

        public static readonly Error InvalidRomanianFormat = new(
            "PhoneNumber.InvalidRomanianFormat",
            "Romanian phone number must start with 0 or +40 and have exactly 10 digits.");
    }
}
