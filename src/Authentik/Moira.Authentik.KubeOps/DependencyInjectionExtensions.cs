using KubeOps.Abstractions.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.KubeOps.SettingsResolvers;
using Moira.Common.Abstractions;

namespace Moira.Authentik.KubeOps;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAuthentikKubeOps(this IServiceCollection services)
    {
        services.AddScoped<IProviderSettingsResolver, AuthentikOidcProviderSettingsResolver>();
        services.AddScoped<IProviderSettingsResolver, AuthentikGroupProviderSettingsResolver>();
        return services;
    }
}