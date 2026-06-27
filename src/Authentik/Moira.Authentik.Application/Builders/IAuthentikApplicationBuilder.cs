using Moira.Authentik.Domain.Applications;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Builders;

public interface IAuthentikApplicationBuilder
{
    AuthentikApplicationV3 Build(IdPOidcApplication application, int? providerId, string? applicationPk);
}