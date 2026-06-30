using System.Collections.Concurrent;
using Flurl;
using Flurl.Http;
using Moira.Authentik.Application.Ports;
using Moira.Authentik.Infrastructure.Authentication;
using Moira.Authentik.Domain;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Infrastructure.Version;

public class AuthentikVersionService(
    IAuthentikAuthenticationService authenticationService) : IAuthentikVersionService
{
    private readonly ConcurrentDictionary<string, (AuthentikVersion Version, DateTime ExpiresAt)> _cache = new();

    public async Task<AuthentikVersion> GetVersionAsync(IdPProvider provider, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(provider.Name, out var cached) && cached.ExpiresAt > DateTime.UtcNow)
        {
            return cached.Version;
        }

        var token = await authenticationService.AcquireTokenAsync(provider, cancellationToken);

        var response = await provider.BaseUrl
            .AppendPathSegments("api/v3/root/config/")
            .WithOAuthBearerToken(token)
            .WithHeader("Accept", "application/json")
            .GetAsync(cancellationToken: cancellationToken)
            .ReceiveJson<AuthentikVersionResponseBody>();

        var version = AuthentikVersion.Parse(response.version);
        _cache[provider.Name] = (version, DateTime.UtcNow.AddMinutes(5));
        return version;
    }
}
