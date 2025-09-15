namespace Moira.Common.Models;

public record IdPProvider(string Namespace, string Name, string Type) : IdPEntityBase(Namespace, Name);