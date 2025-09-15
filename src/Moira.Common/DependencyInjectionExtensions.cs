using Microsoft.Extensions.DependencyInjection;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Common;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCommon(this IServiceCollection services)
    {
        services.AddScoped<IProviderRouter<IdPGroup>, ProviderRouter<IdPGroup>>();
        return services;
    }
}