using Microsoft.Extensions.DependencyInjection;
using Moira.Common.Kubernetes.Secrets;

namespace Moira.Common.Kubernetes;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraCommonKubernetes(this IServiceCollection services)
    {
        services.AddScoped<ISecretService, SecretService>();
        return services;
    }
}