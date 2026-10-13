using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MyYt.Application.Abstractions;
using MyYt.Infrastructure.Common;
using MyYt.Infrastructure.Persistence;
using MyYt.Infrastructure.Persistence.Repositories;
using MyYt.Infrastructure.YouTube;

namespace MyYt.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseMySQL(MySqlConnectionString.Resolve(configuration)));
        services.AddHostedService<DatabaseMigrator>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        services.AddHttpClient<IYouTubeMetadataService, OEmbedMetadataService>(http =>
        {
            http.BaseAddress = new Uri("https://www.youtube.com/");
            http.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
