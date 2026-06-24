using KubeOps.KubernetesClient;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider.OidcProviderSettings;

public class AuthentikOidcProviderSettingsResolver(
    IKubernetesClient client) : IOidcProviderSettingsResolver
{
    private const string SupportedApiVersion = "moira.operator/v1alpha1";
    private const string SupportedKind = "AuthentikOidcApplicationSettings";

    public bool CanResolve(IdPProvider provider, ResourceRef settingsRef)
    {
        return provider.Type.Equals(ProviderType.Authentik.ToString(), StringComparison.OrdinalIgnoreCase) &&
               settingsRef.Kind.Equals(SupportedKind, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<Moira.Common.Models.OidcProviderSettings> ResolveAsync(
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

        var settings = await client.GetAsync<AuthentikOidcApplicationSettings>(
            settingsRef.Name,
            settingsNamespace,
            cancellationToken);

        if (settings is null)
        {
            throw new ProviderSettingsException(
                $"Unable to get AuthentikOidcApplicationSettings with name \"{settingsRef.Name}\" in namespace \"{settingsNamespace}\".");
        }

        return new Moira.Common.Models.OidcProviderSettings(
            settingsRef.Kind,
            new Dictionary<string, string>
            {
                ["authorizationFlowSlug"] = settings.Spec.AuthorizationFlowSlug,
                ["invalidationFlowSlug"] = settings.Spec.InvalidationFlowSlug,
                ["redirectUriMatchingMode"] = settings.Spec.RedirectUriMatchingMode
            });
    }
}
