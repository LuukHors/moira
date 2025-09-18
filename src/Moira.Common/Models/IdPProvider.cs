namespace Moira.Common.Models;

public record IdPProvider(string Namespace, string Name, string Type, string BaseUrl, string ClientId, string ClientSecret) : IdPEntityBase(Namespace, Name);