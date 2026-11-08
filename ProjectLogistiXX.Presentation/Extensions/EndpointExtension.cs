using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectLogistiXX.Presentation.Endpoints;
using System.Reflection;

namespace ProjectLogistiXX.Presentation.Extensions
{
    public static class EndpointExtension
    {
        public static IServiceCollection AddEndpoints(
            this IServiceCollection services,
            Assembly assembly)
        {
            var endpointTypes = assembly.GetTypes()
                 .Where(t =>
                 typeof(IEndpoints).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                 .Select(type => ServiceDescriptor.Transient(typeof(IEndpoints), type))
                 .ToArray();

            services.TryAddEnumerable(endpointTypes);

            return services;
        }

        public static IApplicationBuilder MapEndpoints(
            this WebApplication app,
            RouteGroupBuilder? routeGroupBuilder = null)

        {
            var endpoints = app.Services.GetServices<IEndpoints>();

            IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

            foreach (var endpoint in endpoints)
            {
                endpoint.MapEndpoints(builder);
            }

            return app;
        }
    }
}
