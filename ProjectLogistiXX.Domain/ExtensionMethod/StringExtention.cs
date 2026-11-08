using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.ExtensionMethod
{
    internal static class StringExtention
    {
        private static readonly SearchValues<char> AllowedChars = SearchValues.Create("0123456789");

        public static bool IsDigitsOnly(this ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
            {
                return false;
            }

            return !value.ContainsAnyExcept(AllowedChars);
        }
    }
}
