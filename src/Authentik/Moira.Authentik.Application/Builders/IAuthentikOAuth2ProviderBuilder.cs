using Moira.Authentik.Application.Models;
using Moira.Authentik.Domain.Applications;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Builders;

public interface IAuthentikOAuth2ProviderBuilder
{
    Task<AuthentikOAuth2ProviderV3> BuildAsync(
        AuthentikOidcApplicationModel application,
        string clientId,
        string clientSecret,
        int? providerId,
        CancellationToken cancellationToken);
}