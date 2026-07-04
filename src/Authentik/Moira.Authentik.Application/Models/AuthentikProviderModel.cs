using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Models;

public record AuthentikProviderModel(string Namespace, string Name, string Type, string BaseUrl, string ClientId, string ClientSecret) 
    : IdPProvider(Namespace, Name, Type, BaseUrl, ClientId, ClientSecret);