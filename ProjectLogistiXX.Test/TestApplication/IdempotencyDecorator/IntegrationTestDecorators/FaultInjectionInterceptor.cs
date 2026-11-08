using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProjectLogistiXX.Domain.Entities;

namespace ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator.IntegrationTestDecorators
{
    public enum FaultPoint
    {
        None,
        BeforeUserInsert,
        AfterUserInsertBeforeCommit
    }
    public class FaultInjectionInterceptor : SaveChangesInterceptor
    {
        private bool UserSavePending;

        public FaultPoint Fault { get; set; } = FaultPoint.None;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UserSavePending = AddsUser(eventData.Context);

            if(UserSavePending && Fault == FaultPoint.BeforeUserInsert)
            {
                throw new InvalidOperationException("Simulated DB failure before user insert");
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData, 
            int result, 
            CancellationToken cancellationToken = default)
        {
            if (UserSavePending && Fault == FaultPoint.AfterUserInsertBeforeCommit)
            {
                UserSavePending = false;
                throw new InvalidOperationException("Simulated connection loss after user insert, before commit");
            }

            UserSavePending = false;
            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private static bool AddsUser(DbContext? context)
            => context?.ChangeTracker.Entries<User>().Any(
                e => e.State == EntityState.Added) ?? false;
    }
}
