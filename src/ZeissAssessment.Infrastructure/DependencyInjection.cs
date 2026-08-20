using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZeissAssessment.Application.Interfaces;
using ZeissAssessment.Application.Interfaces.Repositories;
using ZeissAssessment.Infrastructure.Persistence;
using ZeissAssessment.Infrastructure.Repositories;

namespace ZeissAssessment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddRepositories();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connStrings = configuration.GetSection(nameof(ConnectionStrings)).Get<ConnectionStrings>()!;

        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(connStrings.DefaultConnection));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}