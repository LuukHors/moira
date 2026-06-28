using KubeOps.KubernetesClient;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider.GroupProviderSettings;

public class AuthentikGroupProviderSettingsResolver(
    IKubernetesClient client) : IProviderSettingsResolver<Common.Models.GroupProviderSettings>
{
    private const string SupportedApiVersion = "moira.operator/v1alpha1";
    private const string SupportedKind = "AuthentikGroupSettings";

    public bool CanResolve(IdPProvider provider, ResourceRef settingsRef)
    {
        return provider.Type.Equals(ProviderType.Authentik.ToString(), StringComparison.OrdinalIgnoreCase) &&
               settingsRef.Kind.Equals(SupportedKind, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<Common.Models.GroupProviderSettings> ResolveAsync(
        ResourceRef settingsRef,
        string defaultNamespace,
        IdPProvider provider,
        CancellationToken cancellationToken)
    {
        if (!settingsRef.ApiVersion.Equals(SupportedApiVersion, StringComparison.OrdinalIgnoreCase))
        {
            throw new ProviderSettingsException(
                $"Unsupported providerSettingsRef apiVersion \"{settingsRef.ApiVersion}\" for settings kind \"{settingsRef.Kind}\".");
        }

        var settingsNamespace = string.IsNullOrWhiteSpace(settingsRef.Namespace)
            ? defaultNamespace
            : settingsRef.Namespace;

        var settings = await client.GetAsync<AuthentikGroupSettings>(
            settingsRef.Name,
            settingsNamespace,
            cancellationToken);

        if (settings is null)
        {
            throw new ProviderSettingsException(
                $"Unable to get AuthentikGroupSettings with name \"{settingsRef.Name}\" in namespace \"{settingsNamespace}\".");
        }

        return new Common.Models.GroupProviderSettings(
            settingsRef.Kind,
            new Dictionary<string, string>(settings.Spec.Attributes));
    }
}