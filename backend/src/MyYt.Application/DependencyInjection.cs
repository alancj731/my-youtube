using Microsoft.Extensions.DependencyInjection;
using MyYt.Application.Auth;
using MyYt.Application.Tags;
using MyYt.Application.Videos;

namespace MyYt.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<UserProvisioner>();
        services.AddScoped<GetCurrentUserHandler>();

        services.AddScoped<AddVideoHandler>();
        services.AddScoped<GetVideoHandler>();
        services.AddScoped<SearchVideosHandler>();
        services.AddScoped<UpdateVideoTagsHandler>();
        services.AddScoped<DeleteVideoHandler>();

        services.AddScoped<ListTagsHandler>();
        services.AddScoped<RenameTagHandler>();
        services.AddScoped<DeleteTagHandler>();

        return services;
    }
}
