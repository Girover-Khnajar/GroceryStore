using CQRS;
using GroceryStore.Application.Categories.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace GroceryStore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddCqrs(options =>
        {
            options.RegisterServicesFromAssembly(typeof(CreateCategoryCommandHandler).Assembly);
            options.ConfigurePipelineBehaviors(ctx =>
            {
                ctx.UseValidation();
                ctx.UseLogging();
            });
        });

        return services;
    }
}
