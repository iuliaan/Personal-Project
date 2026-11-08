using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator.IntegrationTestDecorators
{
    public sealed class CommitFaultInterceptor : DbTransactionInterceptor
    {
        public bool FailAfterCommit { get; set; }

        public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
        {
            if (FailAfterCommit)
            {
                throw new InvalidOperationException("Simulated network loss after commit");
            }
        }

        public override Task TransactionCommittedAsync(
            DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            if (FailAfterCommit)
            {
                throw new InvalidOperationException("Simulated network loss after commit");
            }

            return Task.CompletedTask;
        }
    }
}
