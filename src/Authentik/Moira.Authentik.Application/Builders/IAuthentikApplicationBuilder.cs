using Moira.Authentik.Application.Models;
using Moira.Authentik.Domain.Applications;

namespace Moira.Authentik.Application.Builders;

public interface IAuthentikApplicationBuilder
{
    AuthentikApplicationV3 Build(AuthentikOidcApplicationModel application, int? providerId, string? applicationPk);
}