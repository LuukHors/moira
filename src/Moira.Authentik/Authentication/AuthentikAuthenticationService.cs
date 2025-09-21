using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Moira.Common.Models;

namespace Moira.Authentik.Authentication;

public class AuthentikAuthenticationService(
    HttpClient httpClient,
    ILogger<AuthentikAuthenticationService> logger) : IAuthentikAuthenticationService
{
    private readonly Dictionary<string, AuthentikToken> _tokens = new();
    
    public async Task<string> AcquireTokenAsync(IdPProvider provider, CancellationToken cancellationToken)
    {
        var tokenCached = _tokens.TryGetValue(provider.Name, out var token);

        if (tokenCached && token is not null && token.ExpiresAt > DateTime.UtcNow.AddMinutes(-3))
        {
            logger.LogDebug("Getting token from cache");
            return token.Token;
        }

        logger.LogDebug("Continue getting token from Authentik api");
        
        var requestContent = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", provider.ClientId),
            new("client_secret", provider.ClientSecret),
            new("scope", "goauthentik.io/api")
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{provider.BaseUrl}/application/o/token/")
        {
            Content = new FormUrlEncodedContent(requestContent)
        };
        
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        logger.LogDebug("Sending /application/o/token request to Authentik API");
        var rawResponse = await httpClient.SendAsync(request, cancellationToken);
        var response = await rawResponse.Content.ReadFromJsonAsync<AuthentikAuthenticationResponseBody>(cancellationToken);

        if (response is null || !rawResponse.IsSuccessStatusCode)
        {
            logger.LogDebug("Token response was not successful, throwing exception..");
            var responseBody = await rawResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Unable to retrieve token ({rawResponse.StatusCode}): {responseBody}");
        }
        
        logger.LogDebug("Setting successful response token as cache entry {providerName}", provider.Name);
        _tokens[provider.Name] = new AuthentikToken(response.access_token, DateTime.UtcNow.AddSeconds(response.expires_in - 180));
        return response.access_token;
    }

    public bool InvalidateCachedToken(string providerName)
    {
        return _tokens.Remove(providerName);
    }
}