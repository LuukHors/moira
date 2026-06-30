using KubeOps.KubernetesClient;
using Moira.Authentik.KubeOps.Entities;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.KubeOps.SettingsResolvers;

public class AuthentikOidcProviderSettingsResolver(IKubernetesClient client) : IProviderSettingsResolver
{
    private const string SupportedApiVersion = "moira.operator/v1alpha1";
    private const string SupportedKind = "AuthentikOidcApplicationSettings";

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

        var settings = await client.GetAsync<AuthentikOidcApplicationSettings>(
            settingsRef.Name,
            settingsNamespace,
            cancellationToken);

        if (settings is null)
        {
            throw new ProviderSettingsException(
                $"Unable to get AuthentikOidcApplicationSettings with name \"{settingsRef.Name}\" in namespace \"{settingsNamespace}\".");
        }

        return new IdpProviderSpecificSettings(
            new Dictionary<string, string>
            {
                ["authorizationFlowSlug"] = settings.Spec.AuthorizationFlowSlug,
                ["invalidationFlowSlug"] = settings.Spec.InvalidationFlowSlug,
                ["redirectUriMatchingMode"] = settings.Spec.RedirectUriMatchingMode,
                ["group"] = settings.Spec.Group,
                ["accessCodeValidity"] = settings.Spec.TokenSettings.AccessCodeValidity,
                ["accessTokenValidity"] = settings.Spec.TokenSettings.AccessTokenValidity,
                ["refreshTokenValidity"] = settings.Spec.TokenSettings.RefreshTokenValidity,
                ["description"] = settings.Spec.MetadataSettings.Description,
                ["icon"] = settings.Spec.MetadataSettings.Icon,
                ["publisher"] = settings.Spec.MetadataSettings.Publisher,
                ["openInNewTab"] = settings.Spec.MetadataSettings.OpenInNewTab.ToString().ToLowerInvariant()
            }
        );
    }
}
