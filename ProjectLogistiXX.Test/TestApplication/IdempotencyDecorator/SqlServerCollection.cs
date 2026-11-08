using System;

namespace ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator
{
    [CollectionDefinition(SqlServerCollection.Name)]
    public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
    {
        public const string Name = "SqlServer";
    }
}
