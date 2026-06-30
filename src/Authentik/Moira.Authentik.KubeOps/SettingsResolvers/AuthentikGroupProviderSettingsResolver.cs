using KubeOps.KubernetesClient;
using Moira.Authentik.KubeOps.Entities;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.KubeOps.SettingsResolvers;

public class AuthentikGroupProviderSettingsResolver(IKubernetesClient client) : IProviderSettingsResolver
{
    private const string SupportedApiVersion = "moira.operator/v1alpha1";
    private const string SupportedKind = "AuthentikGroupSettings";

    public bool CanResolve(ResourceRef settingsRef)
    {
        return settingsRef.Kind.Equals(SupportedKind, StringComparison.Ordinal)
            && settingsRef.ApiVersion.Equals(SupportedApiVersion, StringComparison.Ordinal);
    }

    public async Task<IdpProviderSpecificSettings?> ResolveAsync(ResourceRef settingsRef, CancellationToken cancellationToken)
    {
        var settings = await client.GetAsync<AuthentikGroupSettings>(
            settingsRef.Name,
            settingsRef.Namespace,
            cancellationToken);

        if (settings is null)
        {
            throw new ProviderSettingsException(
                $"Unable to get AuthentikGroupSettings with name \"{settingsRef.Name}\" in namespace \"{settingsRef.Namespace}\".");
        }

        return new IdpProviderSpecificSettings();
    }
}