using Moira.Common.Models;

namespace Moira.Authentik.Authentication;

public interface IAuthentikAuthenticationService
{
    public Task<string> AcquireTokenAsync(IdPProvider provider, CancellationToken cancellationToken);
    public bool InvalidateCachedToken(string providerName);
}