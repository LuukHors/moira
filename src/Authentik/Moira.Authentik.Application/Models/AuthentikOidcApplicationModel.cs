using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Models;

public record AuthentikOidcApplicationModel(
    string Namespace,
    string Name,
    IdPProvider IdPProvider,
    AuthentikOidcApplicationSpec Spec,
    AuthentikOidcApplicationStatus Status,
    string ClientSecret = "") : IdPEntity(Namespace, Name, IdPProvider)
{
    public AuthentikOidcApplicationModel CopyWithNewStatus(AuthentikOidcApplicationStatus status, string clientSecret = "")
    {
        return this with { Status = status, ClientSecret = clientSecret };
    }
}