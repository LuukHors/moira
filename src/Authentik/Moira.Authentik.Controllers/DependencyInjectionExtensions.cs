using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Application;
using Moira.Authentik.Application.Models;
using Moira.Authentik.Controllers.Adapters;
using Moira.Authentik.Infrastructure;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Controllers;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraAuthentikProvider(this IServiceCollection services)
    {
        services
            .AddAuthentikInfrastructure()
            .AddAuthentikApplication();

        services.AddScoped<IProviderAdapter<AuthentikGroupModel>, AuthentikGroupProviderAdapter>();
        services.AddScoped<IProviderAdapter<AuthentikOidcApplicationModel>, AuthentikApplicationProviderAdapter>();
        services.AddScoped<IProviderAdapter<IdPProvider>, AuthentikProviderAdapter>();

        return services;
    }
}