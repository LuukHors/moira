using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Xml.XPath;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Moira.Common.Exceptions;
using Moira.Common.Models;

namespace Moira.Authentik.Authentication;

public class AuthentikAuthenticationService(
    ILogger<AuthentikAuthenticationService> logger) : IAuthentikAuthenticationService
{
    private readonly Dictionary<string, AuthentikToken> _tokens = new();
    
    public async Task<string> AcquireTokenAsync(IdPProvider provider, CancellationToken cancellationToken)
    {
        var tokenCached = _tokens.TryGetValue(provider.Name, out var token);

        if (tokenCached && token is not null && token.ExpiresAt > DateTime.UtcNow.AddMinutes(-1))
        {
            logger.LogDebug("Using cached Authentik token for provider {ProviderName}; token expires at {TokenExpiresAt}", provider.Name, token.ExpiresAt);
            return token.Token;
        }

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
            throw new IdPHttpException(
                "Request was not able to be completed within 10 seconds",
                HttpStatusCode.RequestTimeout,
                "POST",
                endpoint,
                (int)HttpStatusCode.RequestTimeout,
                ex);
        }
        catch (FlurlHttpException ex)
        {
            var message = await ex.GetResponseStringAsync();
            throw new IdPHttpException(message, null, "POST", endpoint, ex.StatusCode, ex);
        }
    }

    public bool InvalidateCachedToken(string providerName)
    {
        return _tokens.Remove(providerName);
    }
}
