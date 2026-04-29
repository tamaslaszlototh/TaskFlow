using System.Reflection;
using Mapster;
using MapsterMapper;

namespace TaskFlow.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiLayerServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddMapper();

        return services;
    }

    private static IServiceCollection AddMapper(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}