using Microsoft.Extensions.DependencyInjection;
using Moira.Common.Abstractions;
using Moira.Common.Services.Provider;

namespace Moira.Common.Services;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraCommon(this IServiceCollection services)
    {
        services.AddScoped(typeof(IProviderRouter<>), typeof(ProviderRouter<>));
        return services;
    }
}