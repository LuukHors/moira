using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Authentication;
using Moira.Authentik.Handlers;
using Moira.Authentik.ProviderAdapters;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAuthentikProvider(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        services.AddSingleton<IAuthentikAuthenticationService, AuthentikAuthenticationService>();
        // services.AddHttpClient();
        return services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IAuthentikHandler<,>)))
            .AsImplementedInterfaces()
            .AddClasses(classes => classes.AssignableTo(typeof(IProviderAdapter<>)))
            .AsImplementedInterfaces());
    }
}