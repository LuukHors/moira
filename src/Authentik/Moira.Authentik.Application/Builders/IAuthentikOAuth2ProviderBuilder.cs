using Moira.Authentik.Domain.Applications;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Builders;

public interface IAuthentikOAuth2ProviderBuilder
{
    Task<AuthentikOAuth2ProviderV3> BuildAsync(
        IdPOidcApplication application,
        string clientId,
        string clientSecret,
        int? providerId,
        CancellationToken cancellationToken);
}