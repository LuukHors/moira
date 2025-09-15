using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.ProviderAdapters;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAuthentikProvider(this IServiceCollection services)
    {
        return services.AddScoped<IProviderAdapter<IdPGroup>, AuthentikGroupProviderAdapter>();
    }
}