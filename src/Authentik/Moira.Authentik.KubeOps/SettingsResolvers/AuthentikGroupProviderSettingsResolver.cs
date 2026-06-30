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
        return settingsRef.Kind.Equals(SupportedKind, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IdpProviderSpecificSettings?> ResolveAsync(ResourceRef settingsRef, CancellationToken cancellationToken)
    {
        if (!settingsRef.ApiVersion.Equals(SupportedApiVersion, StringComparison.OrdinalIgnoreCase))
        {
            throw new ProviderSettingsException(
                $"Unsupported providerSettingsRef apiVersion \"{settingsRef.ApiVersion}\" for settings kind \"{settingsRef.Kind}\".");
        }

        var settingsNamespace = settingsRef.Namespace;

        var settings = await client.GetAsync<AuthentikGroupSettings>(
            settingsRef.Name,
            settingsNamespace,
            cancellationToken);

        if (settings is null)
        {
            throw new ProviderSettingsException(
                $"Unable to get AuthentikGroupSettings with name \"{settingsRef.Name}\" in namespace \"{settingsNamespace}\".");
        }

        return new IdpProviderSpecificSettings();
    }
}