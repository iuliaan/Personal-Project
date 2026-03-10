using ProjectLogistiXX.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Application.Common.Interfaces
{
    public interface IUserRepository<T> where T : class
    {
        Task AddAsync(T entity);

        Task<bool> IsEmaiUniqueAsyns(string email);

        Task<T?> GetByEmailAsync(string email);

        Task SaveChangesAsync();
    }
}
