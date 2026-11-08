using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ProjectLogistiXX.Application.Abstraction.Decorators;
using ProjectLogistiXX.Application.Abstraction.Messaging;

namespace ProjectLogistiXX.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var currentAssembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(currentAssembly));

        services.AddValidatorsFromAssembly(currentAssembly);

        services.Scan(scan => scan
            .FromAssemblies(currentAssembly)
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Decorate(typeof(ICommandHandler<,>), typeof(IdempotentDecorator<,>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator<,>));

        return services;
    }
}
