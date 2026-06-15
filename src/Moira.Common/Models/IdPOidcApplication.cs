namespace Moira.Common.Models;

public record IdPOidcApplication(string Namespace, string Name, IdPProvider IdPProvider) : IdPEntity(Namespace, Name, IdPProvider);