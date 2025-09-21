using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Authentication;
using Moira.Authentik.Handlers;
using Moira.Authentik.HttpService;
using Moira.Authentik.ProviderAdapters;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraAuthentikProvider(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        services.AddSingleton<IAuthentikAuthenticationService, AuthentikAuthenticationService>();
        
        return services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IAuthentikHttpService<,>)))
            .AsImplementedInterfaces()
            .AddClasses(classes => classes.AssignableTo(typeof(IAuthentikHandler<,>)))
            .AsImplementedInterfaces()
            .AddClasses(classes => classes.AssignableTo(typeof(IProviderAdapter<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }
}