using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moira.Common.Models;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.Controllers;
using Moira.KubeOps.DependencyProvider;
using Moira.KubeOps.Entities;
using Moira.KubeOps.ResultHandler;

namespace Moira.KubeOps;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraKubeOps(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddScoped<IAdapterHandler<Group>, AdapterHandler<Group,IdPGroup>>();
        services.AddHttpClient();
        return services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IResultHandler<,>)))
            .AsImplementedInterfaces()
            .AddClasses(c => c.AssignableTo(typeof(IDependencyProvider<,>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());
    }
}