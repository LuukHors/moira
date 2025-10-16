using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moira.Common.Models;
using Moira.Common.RequestContext;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.DependencyProvider;
using Moira.KubeOps.Entities;
using Moira.KubeOps.ResultHandler;
using Moira.KubeOps.ValidatorWebhooks.Executor;

namespace Moira.KubeOps;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraKubeOps(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddScoped<IAdapterHandler<Group>, AdapterHandler<Group,IdPGroup>>();
        
        return services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IResultHandler<,>)))
            .AsImplementedInterfaces()
            .AddClasses(classes => classes.AssignableTo(typeof(IValidatorExecutor<>)))
            .AsSelfWithInterfaces()
            .AddClasses(c => c.AssignableTo(typeof(IDependencyProvider<,>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());
    }
}