using ProjectLogistiXX.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Test.TestDomain
{
    public class TestUser
    {
        private static Email CreateTestEmail(string email = "test@example.com")
            => Email.Create(email).Value;

        private static PhoneNumber CreateTestPhoneNumber(string number = "+40753632081")
            => PhoneNumber.Create(number).Value;

        private static Address CreateTestAddress() => Address.Create
            ("Mihai Test", "123", "apartament 1", "Bucuresti", "Bucuresti","4452000", "Romania").Value;
    }
}
