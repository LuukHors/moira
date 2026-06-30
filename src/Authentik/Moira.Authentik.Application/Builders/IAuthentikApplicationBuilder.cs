using Moira.Authentik.Domain.Applications;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Builders;

public interface IAuthentikApplicationBuilder
{
    AuthentikApplicationV3 Build(IdpProviderSpecificSettings providerSettings, IdPOidcApplication application, int? providerId, string? applicationPk);
}