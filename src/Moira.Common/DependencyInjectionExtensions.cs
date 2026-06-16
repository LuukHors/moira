using Microsoft.Extensions.DependencyInjection;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Common;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraCommon(this IServiceCollection services)
    {
        services.AddScoped<IProviderRouter<IdPGroup>, ProviderRouter<IdPGroup>>();
        services.AddScoped<IProviderRouter<IdPProvider>, ProviderRouter<IdPProvider>>();
        services.AddScoped<IProviderRouter<IdPOidcApplication>, ProviderRouter<IdPOidcApplication>>();
        return services;
    }
}
