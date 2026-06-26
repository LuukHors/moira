using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Application;
using Moira.Authentik.Application.Handlers;
using Moira.Authentik.Controllers.Adapters;
using Moira.Authentik.Domain.Applications;
using Moira.Authentik.Domain.Groups;
using Moira.Authentik.Infrastructure;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik.Controllers;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraAuthentikProvider(this IServiceCollection services)
    {
        services
            .AddAuthentikInfrastructure()
            .AddAuthentikApplication();

        services.AddScoped<IProviderAdapter<IdPGroup>, AuthentikGroupProviderAdapter>();
        services.AddScoped<IProviderAdapter<IdPOidcApplication>, AuthentikApplicationProviderAdapter>();
        services.AddScoped<IProviderAdapter<IdPProvider>, AuthentikProviderAdapter>();

        return services;
    }
}
