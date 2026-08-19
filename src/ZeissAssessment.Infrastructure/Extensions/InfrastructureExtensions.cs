using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZeissAssessment.Infrastructure.Persistence;

namespace ZeissAssessment.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connStrings = configuration.GetSection(nameof(ConnectionStrings)).Get<ConnectionStrings>()!;

        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(connStrings.DefaultConnection));

        return services;
    }
}