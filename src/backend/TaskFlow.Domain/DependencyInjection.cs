using Microsoft.Extensions.DependencyInjection;

namespace TaskFlow.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainLayerServices(this IServiceCollection services)
    {
        return services;
    }
}