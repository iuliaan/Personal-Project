using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ProjectLogistiXX.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.MsSql;

namespace ProjectLogistiXX.Test.TestApplication.IdempotencyDecorator
{
    public sealed class SqlServerFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        public string ConnectionString { get; private set; } = string.Empty;

        public IDbContextFactory<ApplicationDbContext> DbContextFactory { get; private set; } = null!;

        public async Task DisposeAsync() => await _dbContainer.DisposeAsync();


        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            ConnectionString = _dbContainer.GetConnectionString();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_dbContainer.GetConnectionString())
                .Options;

            DbContextFactory = new PooledDbContextFactory<ApplicationDbContext>(optionsBuilder);

            //_setupContext = new ApplicationDbContext(optionsBuilder);
            //await _setupContext.Database.EnsureCreatedAsync();

            await using var dbContext = await DbContextFactory.CreateDbContextAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }
    }
}
