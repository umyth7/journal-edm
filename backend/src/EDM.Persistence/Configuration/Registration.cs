using EDM.Application.Interfaces.Persistence;
using EDM.Persistence.Context;
using EDM.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EDM.Persistence.Configuration;

public static class Registration
{
    public static IServiceCollection UsePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Default"),
                m =>
                {
                    m.MigrationsHistoryTable("__efcore_migrations");
                    m.EnableRetryOnFailure();
                }
            ).UseSnakeCaseNamingConvention()
        );

        services.AddScoped<IArtistRepository, ArtistRepository>();
        services.AddScoped<IContentJobRepository, ContentJobRepository>();
        services.AddScoped<ICaptionRepository, CaptionRepository>();
        return services;
    }
}
