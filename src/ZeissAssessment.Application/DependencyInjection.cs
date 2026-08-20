using Microsoft.Extensions.DependencyInjection;
using ZeissAssessment.Application.Interfaces.Services;
using ZeissAssessment.Application.Mappers;
using ZeissAssessment.Application.Services;

namespace ZeissAssessment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddServices();
        services.AddMappers();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();

        return services;
    }

    private static IServiceCollection AddMappers(this IServiceCollection services)
    {
        services.AddScoped<ProductMapper>();

        return services;
    }
}