using System.Text;
using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.KubeOps.Entities;
using Moira.KubeOps.Secrets;
using Moira.KubeOps.Secrets.Models;
using Provider = Moira.KubeOps.Entities.Provider;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider;

public class OidcApplicationDependencyProvider(
    IKubernetesClient client,
    IDependencyProvider<Provider, IdPProvider> providerDependencyProvider,
    ILogger<OidcApplicationDependencyProvider> logger) : IDependencyProvider<OidcApplication, IdPOidcApplication>
{
    public async Task<IdPOidcApplication> ResolveAsync(OidcApplication entity, CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Resolving provider {ProviderNamespace}/{ProviderName} for OIDC application",
            entity.Spec.ProviderRef.Namespace,
            entity.Spec.ProviderRef.Name);

        var provider = await client.GetAsync<Provider>(
            entity.Spec.ProviderRef.Name,
            entity.Spec.ProviderRef.Namespace,
            cancellationToken);

        if (provider is null)
        {
            logger.LogDebug(
                "Provider {ProviderNamespace}/{ProviderName} was not found",
                entity.Spec.ProviderRef.Namespace,
                entity.Spec.ProviderRef.Name);
            throw new ProviderNotFoundException(entity.Spec.ProviderRef.Namespace, entity.Spec.ProviderRef.Name);
        }

        var idPProvider = await providerDependencyProvider.ResolveAsync(provider, cancellationToken);
        var providerSettings = await ResolveProviderSettingsAsync(entity, idPProvider, cancellationToken);
        logger.LogDebug(
            "Resolved provider {ProviderNamespace}/{ProviderName} for OIDC application",
            entity.Spec.ProviderRef.Namespace,
            entity.Spec.ProviderRef.Name);

        var sourceSecret = await client.GetAsync<V1Secret>(
            OidcSecretNames.SourceSecretName(entity),
            entity.Namespace(),
            cancellationToken);

        var clientSecret = sourceSecret?.Data is not null &&
                           sourceSecret.Data.TryGetValue("ClientSecret", out var clientSecretBytes)
            ? Encoding.UTF8.GetString(clientSecretBytes)
            : string.Empty;

        var shouldRotate = entity.Status.NextRotationAt.HasValue &&
                           entity.Status.NextRotationAt.Value <= DateTime.UtcNow &&
                           !string.IsNullOrEmpty(entity.Status.ApplicationId) &&
                           entity.Spec.Oidc.ClientAuthenticationMethod != OidcClientAuthenticationMethod.None;

        return new IdPOidcApplication(
            entity.Namespace(),
            entity.Name(),
            idPProvider,
            new IdPOidcApplicationSpec(
                entity.Spec.DisplayName,
                entity.Spec.Oidc.ApplicationType,
                entity.Spec.Oidc.RedirectUris,
                entity.Spec.Oidc.PostLogoutRedirectUris,
                entity.Spec.Oidc.LaunchUrl,
                entity.Spec.Oidc.Scopes,
                entity.Spec.Oidc.GrantTypes,
                entity.Spec.Oidc.ResponseTypes,
                entity.Spec.Oidc.ClientAuthenticationMethod,
                entity.Spec.Oidc.ClientUri,
                entity.Spec.Oidc.LogoUri,
                entity.Spec.Oidc.PolicyUri,
                entity.Spec.Oidc.TermsOfServiceUri,
                entity.Spec.Oidc.Contacts,
                providerSettings,
                entity.Spec.AutoDelete,
                entity.Spec.RotationDays,
                shouldRotate),
            new IdPOidcApplicationStatus(
                entity.Status.ApplicationId,
                entity.Status.ClientId,
                entity.Status.LastRotatedAt,
                entity.Status.NextRotationAt,
                entity.Status.ProviderResourceIds),
            clientSecret);
    }

    private async Task<OidcProviderSettings?> ResolveProviderSettingsAsync(
        OidcApplication entity,
        IdPProvider provider,
        CancellationToken cancellationToken)
    {
        if (entity.Spec.ProviderSettingsRef is null)
        {
            return null;
        }

        var settingsRef = entity.Spec.ProviderSettingsRef;
        var settingsNamespace = string.IsNullOrWhiteSpace(settingsRef.Namespace)
            ? entity.Namespace()
            : settingsRef.Namespace;

        if (!settingsRef.ApiVersion.Equals("moira.operator/v1alpha1", StringComparison.OrdinalIgnoreCase))
        {
            throw new ProviderSettingsException(
                $"Unsupported providerSettingsRef apiVersion \"{settingsRef.ApiVersion}\" for OIDC application \"{entity.Name()}\".");
        }

        if (provider.Type.Equals(ProviderType.Authentik.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            if (!settingsRef.Kind.Equals("AuthentikOidcApplicationSettings", StringComparison.OrdinalIgnoreCase))
            {
                throw new ProviderSettingsException(
                    $"Provider \"{provider.Name}\" has type \"{provider.Type}\" and requires providerSettingsRef.kind \"AuthentikOidcApplicationSettings\".");
            }

            var settings = await client.GetAsync<AuthentikOidcApplicationSettings>(
                settingsRef.Name,
                settingsNamespace,
                cancellationToken);

            if (settings is null)
            {
                throw new ProviderSettingsException(
                    $"Unable to get AuthentikOidcApplicationSettings with name \"{settingsRef.Name}\" in namespace \"{settingsNamespace}\".");
            }

            return new OidcProviderSettings(
                settingsRef.Kind,
                new Dictionary<string, string>
                {
                    ["authorizationFlowSlug"] = settings.Spec.AuthorizationFlowSlug,
                    ["invalidationFlowSlug"] = settings.Spec.InvalidationFlowSlug,
                    ["redirectUriMatchingMode"] = settings.Spec.RedirectUriMatchingMode
                });
        }

        throw new ProviderSettingsException(
            $"Provider type \"{provider.Type}\" does not support providerSettingsRef on OIDC applications.");
    }
}
