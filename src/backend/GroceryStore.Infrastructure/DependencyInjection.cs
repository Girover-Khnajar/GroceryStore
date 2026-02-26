using GroceryStore.Application.Products;
using GroceryStore.Domain.Interfaces;
using GroceryStore.Infrastructure.Persistence;
using GroceryStore.Infrastructure.Persistence.Catalog.Repositories;
using GroceryStore.Infrastructure.Persistence.Media.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GroceryStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IImageAssetRepository, ImageAssetRepository>();
        services.AddScoped<IProductsReadStore, ProductsReadStore>();

        return services;
    }
}
