namespace Moira.Common.Models;

public record IdPGroup(
    string Namespace,
    string Name,
    IdPProvider IdPProvider,
    IdPGroupSpec Spec,
    IdPGroupStatus Status) : IdPEntity(Namespace, Name, IdPProvider);