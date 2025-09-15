namespace Moira.Common.Models;

public record IdPEntity(string Namespace, string Name, IdPProvider IdPProvider) : IdPEntityBase(Namespace, Name);