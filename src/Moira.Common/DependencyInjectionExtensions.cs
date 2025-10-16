using Microsoft.Extensions.DependencyInjection;
using Moira.Common.Models;
using Moira.Common.Provider;
using Moira.Common.RequestContext;

namespace Moira.Common;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraCommon(this IServiceCollection services)
    {
        services.AddScoped<IProviderRouter<IdPGroup>, ProviderRouter<IdPGroup>>();
        services.AddScoped<IRequestContextProvider, RequestContextProvider>();
        return services;
    }
}