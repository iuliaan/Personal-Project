using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.Interfaces
{
    public interface IPasswordHash
    {
        string HashPassword(string password);

        bool VerifyPassword(string hashedPassword, string providedPassword);
    }
}
