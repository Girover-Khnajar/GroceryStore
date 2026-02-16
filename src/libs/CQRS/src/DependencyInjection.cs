using System.Reflection;
using CQRS.Abstractions.Messaging;
using CQRS.Infrastructure;
using CQRS.Infrastructure.Options;
using CQRS.Infrastructure.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS;

/// <summary>
/// Extension methods for registering CQRS infrastructure
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all CQRS handlers and infrastructure using assembly scanning
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for handlers (defaults to calling assembly)</param>
    /// <returns>The service collection for chaining</returns>

    extension(IServiceCollection services)
    {
        public IServiceCollection AddCqrs(Action<ICqrsOptions> config = null!)
        {
            services.AddTransient<IMessageDispatcher, MessageDispatcher>();

            var options = new CqrsOptions();

            config?.Invoke(options);

            foreach (var assembly in options.AssembliesToScan)
            {
                RegisterHandlersFromAssembly(services, assembly);
            }

            if (options.PipelineOptions.UseLogging)
            {
                services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            }

            if (options.PipelineOptions.UsePerformance)
            {
                services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            }

            if (options.PipelineOptions.UseValidation)
            {
                services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            }

            if (options.PipelineOptions.UseAuthorization)
            {
                services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
            }

            return services;
        }
    }

    private static void RegisterHandlersFromAssembly(IServiceCollection services, Assembly assembly)
    {
        var seen = new HashSet<(Type Service, Type Implementation)>();
        var handlerPairs = new List<(Type Service, Type Implementation)>();

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType)
                {
                    continue;
                }

                var genericDef = iface.GetGenericTypeDefinition();

                if ( genericDef == typeof(ICommandHandler<>)
                  || genericDef == typeof(ICommandHandler<,>) 
                  || genericDef == typeof(IQueryHandler<,>))
                {
                    var key = (Service: iface, Implementation: type);

                    if (seen.Add(key)) // add only if unique
                    {
                        handlerPairs.Add(key);
                    }
                }
            }
        }

        // Register services
        foreach (var (service, implementation) in handlerPairs)
        {
            //Console.WriteLine($"Registering handler: {implementation.FullName} for service: {service.FullName}");
            services.AddTransient(service, implementation);
        }
    }
}