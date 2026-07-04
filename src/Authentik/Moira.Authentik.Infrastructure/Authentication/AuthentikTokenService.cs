using System.Collections.Concurrent;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Infrastructure.Authentication;

public class AuthentikTokenService() : IAuthentikAuthenticationService
{
    public Task<string> AcquireTokenAsync(IdPProvider provider, CancellationToken cancellationToken)
    {
        return Task.FromResult(provider.ClientSecret);
    }

    public bool InvalidateCachedToken(string providerName)
    {
        return true;
    }
}
