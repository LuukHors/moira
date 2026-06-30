namespace Moira.Common.Abstractions.Models;

public record IdPOidcApplication(
    string Namespace,
    string Name,
    IdPProvider IdPProvider,
    IdPOidcApplicationSpec Spec,
    IdPOidcApplicationStatus Status,
    string ClientSecret = "") : IdPEntity(Namespace, Name, IdPProvider)
{
    public IdPOidcApplication CopyWithNewStatus(IdPOidcApplicationStatus status, string clientSecret = "")
    {
        return this with { Status = status, ClientSecret = clientSecret };
    }
}
