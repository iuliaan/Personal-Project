using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectLogistiXX.Application.Abstraction.Data;
using ProjectLogistiXX.Domain.Abstraction;
using ProjectLogistiXX.Domain.Interfaces;
using ProjectLogistiXX.Infrastructure.Persistence;
using ProjectLogistiXX.Infrastructure.Security;
using ProjectLogistiXX.Infrastructure.Persistence.Services;
namespace ProjectLogistiXX.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' was not found.");

        services.AddScoped<IRequestContext, ContextRequest.RequestContext>();

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddDbContext<ApplicationDbContext>(
            options => options.UseSqlServer(connectionString),
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IIdempotencyService<>), typeof(IdempotencyService<>));
        services.AddSingleton(TimeProvider.System);
        services.Configure<IdempotencyOptions>(options =>
            configuration.GetSection(IdempotencyOptions.SectionName).Bind(options));

        services.AddSingleton<IPasswordHash, PasswordHash>();
        services.Configure<PasswordHashOptions>(options =>
            configuration.GetSection("PasswordHash").Bind(options));
        services.AddHostedService<IdempotencyCleanupService>();

        return services;
    }
}
