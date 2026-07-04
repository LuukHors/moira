using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Models;

public record AuthentikGroupModel(
    string Namespace,
    string Name,
    IdPProvider IdPProvider,
    AuthentikGroupSpec Spec,
    AuthentikGroupStatus Status) : IdPEntity(Namespace, Name, IdPProvider)
{
    public AuthentikGroupModel CopyWithNewStatus(AuthentikGroupStatus status)
    {
        return this with { Status = status };
    }
}