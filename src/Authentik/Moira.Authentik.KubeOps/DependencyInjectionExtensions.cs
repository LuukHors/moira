using Microsoft.Extensions.DependencyInjection;

namespace Moira.Authentik.KubeOps;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAuthentikKubeOps(this IServiceCollection services)
    {
        return services;
    }
}