using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectLogistiXX.Domain.Entities;

namespace ProjectLogistiXX.Application.Abstraction.Data
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }

        DbSet<IdempotentRequest> IdempotentRequests { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
