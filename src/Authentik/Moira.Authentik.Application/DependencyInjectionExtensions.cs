using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Application.Handlers;
using Moira.Authentik.Domain.Applications;
using Moira.Authentik.Domain.Groups;
using Moira.Common.Models;

namespace Moira.Authentik.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAuthentikApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthentikHandler<IdPGroup, AuthentikGroupV3>, AuthentikGroupHandler>();
        services.AddScoped<IAuthentikOidcApplicationHandler, AuthentikOidcApplicationHandler>();
        services.AddScoped<IAuthentikHandler<IdPOidcApplication, AuthentikOidcApplicationV3>, AuthentikOidcApplicationHandler>();

        return services;
    }
}
