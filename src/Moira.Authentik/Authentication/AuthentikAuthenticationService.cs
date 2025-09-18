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

        if (tokenCached && token is not null && token.ExpiresAt < DateTime.UtcNow.AddHours(-1))
        {
            return token.Token;
        }

        var requestContent = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", provider.ClientId),
            new("client_secret", provider.ClientSecret),
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{provider.BaseUrl}/application/o/token/")
        {
            Content = new FormUrlEncodedContent(requestContent)
        };
        
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        var rawResponse = await httpClient.SendAsync(request, cancellationToken);
        var response = await rawResponse.Content.ReadFromJsonAsync<AuthentikAuthenticationResponseBody>(cancellationToken);

        if (response is null || !rawResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Unable to retrieve token");
        }
        
        _tokens[provider.Name] = new AuthentikToken(response.access_token, DateTime.FromOADate(response.expires_in));
        return response.access_token;
    }

    public bool InvalidateCachedToken(string providerName)
    {
        return _tokens.Remove(providerName);
    }
}