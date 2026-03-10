

using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace ProjectLogistiXX.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        var currentAssembly = typeof(DependencyInjection).Assembly;

        return services;
    }
}
