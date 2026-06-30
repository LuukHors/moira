using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Infrastructure.Authentication;

public interface IAuthentikAuthenticationService
{
    public Task<string> AcquireTokenAsync(IdPProvider provider, CancellationToken cancellationToken);
    public bool InvalidateCachedToken(string providerName);
}