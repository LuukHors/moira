using System.Text;
using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Models;
using Moira.Authentik.Domain.ProviderSettings;
using Moira.Authentik.KubeOps.Entities;
using Moira.Authentik.KubeOps.Secrets.Models;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;
using Moira.Common.KubeOps.AdapterHandler.DependencyProvider;

namespace Moira.Authentik.KubeOps.AdapterHandler.DependencyProvider;

public class OidcApplicationDependencyProvider(
    IKubernetesClient client,
    IDependencyProvider<AuthentikProvider, IdPProvider> providerDependencyProvider,
    ILogger<OidcApplicationDependencyProvider> logger) : IDependencyProvider<AuthentikOidcApplication, AuthentikOidcApplicationModel>
{
    public async Task<AuthentikOidcApplicationModel> ResolveAsync(AuthentikOidcApplication entity, CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Resolving provider {ProviderNamespace}/{ProviderName} for OIDC application",
            entity.Spec.ProviderRef.Namespace,
            entity.Spec.ProviderRef.Name);

        var provider = await client.GetAsync<AuthentikProvider>(
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

        var settings = new OidcAuthentikProviderSettings(
            InvalidationFlowSlug: entity.Spec.InvalidationFlowSlug,
            AuthorizationFlowSlug: entity.Spec.AuthorizationFlowSlug,
            RedirectUriMatchingMode: entity.Spec.RedirectUriMatchingMode,
            Group: entity.Spec.Group)
        {
            Metadata = new AuthentikApplicationMetadataSettings(
                Description: entity.Spec.MetadataSettings.Description,
                Icon: entity.Spec.MetadataSettings.Icon,
                Publisher: entity.Spec.MetadataSettings.Publisher,
                OpenInNewTab: entity.Spec.MetadataSettings.OpenInNewTab),
            TokenSettings = new AuthentikOauth2ProviderTokenSettings(
                AccessCodeValidity: entity.Spec.TokenSettings.AccessCodeValidity,
                AccessTokenValidity: entity.Spec.TokenSettings.AccessTokenValidity,
                RefreshTokenValidity: entity.Spec.TokenSettings.RefreshTokenValidity)
        };

        return new AuthentikOidcApplicationModel(
            entity.Namespace(),
            entity.Name(),
            idPProvider,
            new AuthentikOidcApplicationSpec(
                entity.Spec.DisplayName,
                entity.Spec.Oidc.ApplicationType,
                entity.Spec.Oidc.RedirectUris,
                entity.Spec.Oidc.LogoutUri,
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
                entity.Spec.AutoDelete,
                entity.Spec.RotationDays,
                shouldRotate,
                settings),
            new AuthentikOidcApplicationStatus(
                entity.Status.ApplicationId,
                entity.Status.ClientId,
                entity.Status.LastRotatedAt,
                entity.Status.NextRotationAt,
                entity.Status.ProviderResourceIds),
            clientSecret);
    }
}