using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Serilog;
using ZeissAssessment.Application;
using ZeissAssessment.Filters;
using ZeissAssessment.Infrastructure;
using ZeissAssessment.Infrastructure.Extensions;
using ZeissAssessment.Middleware;

namespace ZeissAssessment;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;

        builder.Host.UseSerilog((context, services, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName();
        });

        builder.Services.AddControllers(options => options.Filters.Add<ValidateModelFilter>());

        builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ZeissAssessment API",
            });
        });

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddOpenApi();

        builder.Services.AddInfrastructure(configuration);
        builder.Services.AddApplication();

        var app = builder.Build();

        // Standalone migration mode: run as a dedicated release/init step (e.g. `dotnet ZeissAssessment.dll --migrate`)
        // against the target environment's database, then exit. This keeps schema changes an explicit,
        // auditable deploy action instead of something that happens implicitly whenever an app instance boots.
        if (args.Contains("--migrate"))
        {
            Log.Information("Running in migration-only mode");
            await app.MigrateDatabaseAsync();
            return;
        }

        if (app.Environment.IsDevelopment())
        {
            // Convenience for local development only: keep the local DB schema in sync automatically
            // and seed sample data, so `dotnet run` "just works" without a separate migration step.
            await app.MigrateDatabaseAsync();
            await app.SeedDevelopmentDataAsync();
        }

        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseExceptionHandler();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}