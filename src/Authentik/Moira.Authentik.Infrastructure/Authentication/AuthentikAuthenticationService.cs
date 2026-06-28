using System.Collections.Concurrent;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Moira.Common.Exceptions;
using Moira.Common.Models;

namespace Moira.Authentik.Infrastructure.Authentication;

public class AuthentikAuthenticationService(
    ILogger<AuthentikAuthenticationService> logger) : IAuthentikAuthenticationService
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _tokenLocks = new();
    private readonly ConcurrentDictionary<string, AuthentikToken> _tokens = new();

    public async Task<string> AcquireTokenAsync(IdPProvider provider, CancellationToken cancellationToken)
    {
        if (TryGetValidToken(provider.Name, out var cachedToken))
        {
            return cachedToken;
        }

        var tokenLock = _tokenLocks.GetOrAdd(provider.Name, _ => new SemaphoreSlim(1, 1));
        await tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (TryGetValidToken(provider.Name, out cachedToken))
            {
                return cachedToken;
            }

            return await RequestTokenAsync(provider, cancellationToken);
        }
        finally
        {
            tokenLock.Release();
        }
    }

    public bool InvalidateCachedToken(string providerName)
    {
        return _tokens.TryRemove(providerName, out _);
    }

    private bool TryGetValidToken(string providerName, out string tokenValue)
    {
        tokenValue = string.Empty;

        if (!_tokens.TryGetValue(providerName, out var token) ||
            token.ExpiresAt <= DateTime.UtcNow.AddMinutes(-1))
        {
            return false;
        }

        logger.LogDebug(
            "Using cached Authentik token for provider {ProviderName}; token expires at {TokenExpiresAt}",
            providerName,
            token.ExpiresAt);
        tokenValue = token.Token;
        return true;
    }

    private async Task<string> RequestTokenAsync(IdPProvider provider, CancellationToken cancellationToken)
    {
        logger.LogDebug("Requesting new Authentik token for provider {ProviderName}", provider.Name);

        var endpoint = string.Empty;

        try
        {
            var requestContent = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("client_id", provider.ClientId),
                new("client_secret", provider.ClientSecret),
                new("scope", "goauthentik.io/api")
            };

            var url = provider.BaseUrl
                .AppendPathSegment("/application/o/token/")
                .WithHeader("Accept", "application/json")
                .WithTimeout(10);

            endpoint = url.Url.ToString();

            var result = await url.PostUrlEncodedAsync(requestContent, cancellationToken: cancellationToken)
                .ReceiveJson<AuthentikAuthenticationResponseBody>();

            _tokens[provider.Name] = new AuthentikToken(result.access_token, DateTime.UtcNow.AddSeconds(result.expires_in - 60));
            logger.LogDebug("Cached Authentik token for provider {ProviderName}; token expires in {TokenExpiresInSeconds} seconds", provider.Name, result.expires_in);
            return result.access_token;
        }
        catch (FlurlHttpTimeoutException ex)
        {
            throw new IdPException(
                "Request was not able to be completed within 10 seconds",
                IdPExceptionReason.IdpRequestFailed,
                ex);
        }
        catch (FlurlHttpException ex)
        {
            var message = await ex.GetResponseStringAsync();
            if (IsCredentialFailure(ex.StatusCode, message))
            {
                throw new IdPException(
                    string.IsNullOrWhiteSpace(message)
                        ? "Authentik credentials could not be used to acquire an API token."
                        : message,
                    IdPExceptionReason.IdpValidationFailed,
                    ex);
            }

            throw new IdPException(message, IdPExceptionReason.IdpRequestFailed, ex);
        }
    }

    private static bool IsCredentialFailure(int? statusCode, string message)
    {
        return statusCode is 400 or 401 or 403
               && message.Contains("invalid_client", StringComparison.OrdinalIgnoreCase);
    }
}
