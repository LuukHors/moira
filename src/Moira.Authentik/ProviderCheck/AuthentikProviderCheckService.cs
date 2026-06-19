using System.Net;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Authentication;
using Moira.Common.Exceptions;
using Moira.Common.Models;

namespace Moira.Authentik.ProviderCheck;

public class AuthentikProviderCheckService(
    IAuthentikAuthenticationService authenticationService,
    ILogger<AuthentikProviderCheckService> logger) : IAuthentikProviderCheckService
{
    private const int TimeoutSeconds = 10;

    public async Task CheckAsync(IdPProvider provider, CancellationToken cancellationToken)
    {
        logger.LogDebug("Checking Authentik provider {ProviderName}", provider.Name);

        if (string.IsNullOrWhiteSpace(provider.BaseUrl))
        {
            logger.LogDebug("Authentik provider {ProviderName} does not have a baseUrl", provider.Name);
            throw new IdPException("Provider baseUrl should not be empty.", IdPExceptionReason.IdpValidationFailed);
        }

        await CheckHealthAsync(provider, cancellationToken);

        logger.LogDebug("Acquiring Authentik API token for provider {ProviderName}", provider.Name);
        var token = await authenticationService.AcquireTokenAsync(provider, cancellationToken);
        logger.LogDebug("Acquired Authentik API token for provider {ProviderName}", provider.Name);

        await CheckAuthenticatedApiAsync(provider, token, cancellationToken);

        logger.LogInformation(
            "Authentik provider {ProviderName} is reachable and API credentials are usable",
            provider.Name);
        logger.LogDebug("Checked Authentik provider {ProviderName}", provider.Name);
    }

    private async Task CheckHealthAsync(IdPProvider provider, CancellationToken cancellationToken)
    {
        var request = provider.BaseUrl
            .AppendPathSegment("/-/health/ready/")
            .WithHeader("Accept", "application/json")
            .WithTimeout(TimeoutSeconds);

        try
        {
            logger.LogDebug("Checking Authentik health endpoint {Endpoint} for provider {ProviderName}", request.Url, provider.Name);
            var result = await request.GetAsync(cancellationToken: cancellationToken);
            logger.LogDebug(
                "Checked Authentik health endpoint {Endpoint} for provider {ProviderName} with status {StatusCode}",
                request.Url,
                provider.Name,
                result.StatusCode);
        }
        catch (FlurlHttpTimeoutException ex)
        {
            logger.LogDebug(
                ex,
                "Authentik health endpoint {Endpoint} timed out after {TimeoutSeconds} seconds for provider {ProviderName}",
                request.Url,
                TimeoutSeconds,
                provider.Name);
            throw new IdPHttpException(
                $"Request was not able to be completed within {TimeoutSeconds} seconds",
                HttpStatusCode.RequestTimeout,
                "GET",
                request.Url,
                (int)HttpStatusCode.RequestTimeout,
                ex);
        }
        catch (FlurlHttpException ex)
        {
            logger.LogDebug(
                ex,
                "Authentik health endpoint {Endpoint} failed for provider {ProviderName} with status {StatusCode}",
                request.Url,
                provider.Name,
                ex.StatusCode);
            throw await WrapRequestExceptionAsync(ex, "GET", request.Url);
        }
    }

    private async Task CheckAuthenticatedApiAsync(IdPProvider provider, string token, CancellationToken cancellationToken)
    {
        var request = provider.BaseUrl
            .AppendPathSegments("api/v3/core/users/me/")
            .WithOAuthBearerToken(token)
            .WithHeader("Accept", "application/json")
            .WithTimeout(TimeoutSeconds);

        try
        {
            logger.LogDebug("Checking Authentik authenticated endpoint {Endpoint} for provider {ProviderName}", request.Url, provider.Name);
            var result = await request.GetAsync(cancellationToken: cancellationToken);
            logger.LogDebug(
                "Checked Authentik authenticated endpoint {Endpoint} for provider {ProviderName} with status {StatusCode}",
                request.Url,
                provider.Name,
                result.StatusCode);
        }
        catch (FlurlHttpTimeoutException ex)
        {
            logger.LogDebug(
                ex,
                "Authentik authenticated endpoint {Endpoint} timed out after {TimeoutSeconds} seconds for provider {ProviderName}",
                request.Url,
                TimeoutSeconds,
                provider.Name);
            throw new IdPHttpException(
                $"Request was not able to be completed within {TimeoutSeconds} seconds",
                HttpStatusCode.RequestTimeout,
                "GET",
                request.Url,
                (int)HttpStatusCode.RequestTimeout,
                ex);
        }
        catch (FlurlHttpException ex) when (IsAuthorizationFailure(ex.StatusCode))
        {
            var message = await ex.GetResponseStringAsync();
            logger.LogDebug(
                ex,
                "Authentik authenticated endpoint {Endpoint} rejected credentials for provider {ProviderName} with status {StatusCode}",
                request.Url,
                provider.Name,
                ex.StatusCode);
            throw new IdPException(
                string.IsNullOrWhiteSpace(message)
                    ? "Authentik credentials could not be used to call the API."
                    : message,
                IdPExceptionReason.IdpValidationFailed,
                ex);
        }
        catch (FlurlHttpException ex)
        {
            logger.LogDebug(
                ex,
                "Authentik authenticated endpoint {Endpoint} failed for provider {ProviderName} with status {StatusCode}",
                request.Url,
                provider.Name,
                ex.StatusCode);
            throw await WrapRequestExceptionAsync(ex, "GET", request.Url);
        }
    }

    private static async Task<IdPHttpException> WrapRequestExceptionAsync(FlurlHttpException ex, string method, string url)
    {
        var message = await ex.GetResponseStringAsync();
        return new IdPHttpException(message, null, method, url, ex.StatusCode, ex);
    }

    private static bool IsAuthorizationFailure(int? statusCode)
    {
        return statusCode is 401 or 403;
    }
}
