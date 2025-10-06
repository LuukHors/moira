using System.Text.Json;
using Flurl.Http;
using Moira.Authentik.Authentication;
using Moira.Authentik.Models;
using Moira.Common.Exceptions;
using Moira.Common.Models;

namespace Moira.Authentik.HttpService;

public class AuthentikHttpService<TModel, TModelWrite, TId>(
    IAuthentikAuthenticationService authService,
    IAuthentikRoute<TModel, TModelWrite, TId> route
) : IHttpService<TModel, TModelWrite, TId>
{
    private const string AuthentikApiBasePath = "api/v3";
    
    public async Task<AuthentikPageResult<TModel>> ListAsync(string? name, IReadOnlyDictionary<string, object>? attributes, IdPProvider provider, TId? id = default, CancellationToken cancellationToken = default)
    {
        var request = await BuildUrl(provider, id, cancellationToken);

        try
        {
            if (attributes is not null)
                request.AppendQueryParam("attributes", JsonSerializer.Serialize(attributes));

            if (!string.IsNullOrEmpty(name))
                request.AppendQueryParam("name", name);

            return await request
                .GetAsync(cancellationToken: cancellationToken)
                .ReceiveJson<AuthentikPageResult<TModel>>();
        }
        catch (FlurlHttpException e) { throw await WrapAsync(e, "GET", request.Url); }
    }

    public async Task<TModel?> GetByNameAsync(string name, IdPProvider provider, IReadOnlyDictionary<string, object>? attributes, CancellationToken cancellationToken)
    {
        var page = await ListAsync(name, attributes, provider, default, cancellationToken);
        return page.Results.FirstOrDefault();
    }
    
    public async Task<TModel?> GetByIdAsync(TId id, IdPProvider provider, IReadOnlyDictionary<string, object>? attributes, CancellationToken cancellationToken)
    {
        var request = await BuildUrl(provider, id, cancellationToken);

        try
        {
            if (attributes is not null)
                request.AppendQueryParam("attributes", JsonSerializer.Serialize(attributes));
            
            return await request
                .GetAsync(cancellationToken: cancellationToken)
                .ReceiveJson<TModel>();
        }
        catch (FlurlHttpException e) when (e.StatusCode.Equals(404))
        {
            return default;
        }
        catch (FlurlHttpException e)
        {
            throw await WrapAsync(e, "GET", request.Url);
        }
    }

    public async Task<TModel> CreateAsync(TModelWrite model, IdPProvider provider, CancellationToken cancellationToken)
    {
        var request = await BuildUrl(provider, ct: cancellationToken);
        try
        {
            return await request
                .PostJsonAsync(model, cancellationToken: cancellationToken)
                .ReceiveJson<TModel>();
        } 
        catch (FlurlHttpException e) { throw await WrapAsync(e, "POST", request.Url); }
    }

    public async Task<TModel> UpdateAsync(TId id, TModelWrite model, IdPProvider provider, CancellationToken cancellationToken)
    {
        var request = await BuildUrl(provider, id, ct: cancellationToken);
        try
        {
            return await request
                .PutJsonAsync(model, cancellationToken: cancellationToken)
                .ReceiveJson<TModel>();
        } 
        catch (FlurlHttpException e) { throw await WrapAsync(e, "PUT", request.Url); }
    }

    public async Task<bool> DeleteAsync(TId id, IdPProvider provider, CancellationToken cancellationToken)
    {
        var request = await BuildUrl(provider, id, ct: cancellationToken);
        try
        {
            var result = await request
                .DeleteAsync(cancellationToken: cancellationToken);
            
            return result.StatusCode is >= 200 and < 300;
        } 
        catch (FlurlHttpException e) { throw await WrapAsync(e, "DELETE", request.Url); }
    }

    private async Task<IFlurlRequest> BuildUrl(IdPProvider provider, TId? id = default, CancellationToken ct = default)
    {
        var token = await authService.AcquireTokenAsync(provider, ct);
        var relativePath = id is null ? route.CollectionEntityPath : route.SingleEntityPath(id);
        
        return provider.BaseUrl
            .WithOAuthBearerToken(token)
            .AppendPathSegments(AuthentikApiBasePath)
            .AppendPathSegments(relativePath)
            .WithHeader("Accept", "application/json");
    }

    private static async Task<HttpException> WrapAsync(FlurlHttpException ex, string verb, string path)
    {
        var body = await ex.GetResponseStringAsync();
        return new HttpException(body, null, verb, path, ex.StatusCode);
    }
}