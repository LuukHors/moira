using Microsoft.Extensions.DependencyInjection;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Models;
using Moira.Common.Services.Provider;

namespace Moira.Common.Services;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraCommon(this IServiceCollection services)
    {
        services.AddScoped<IProviderRouter<IdPGroup>, ProviderRouter<IdPGroup>>();
        services.AddScoped<IProviderRouter<IdPProvider>, ProviderRouter<IdPProvider>>();
        services.AddScoped<IProviderRouter<IdPOidcApplication>, ProviderRouter<IdPOidcApplication>>();

        services.AddScoped<IProviderSettingsService, ProviderSettingsService>();
        return services;
    }
}
