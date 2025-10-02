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
    public async Task<TModel?> GetAsync(object options, IdPProvider provider, CancellationToken cancellationToken)
    {
        var request = await BuildUrl(provider, route.CollectionPath, cancellationToken);
        
        return await request
            .GetAsync(cancellationToken: cancellationToken)
            .ReceiveJson<TModel>();
    }

    public async Task<TModel?> FindAsync(string? name, IReadOnlyDictionary<string, object>? attributes, IdPProvider provider, CancellationToken cancellationToken)
    {
        var request = await BuildUrl(provider, route.CollectionPath, cancellationToken);

        try
        {
            if (attributes is not null)
                request.AppendQueryParam("attributes", JsonSerializer.Serialize(attributes));

            if (!string.IsNullOrEmpty(name))
                request.AppendQueryParam("name", name);

            var results = await request
                .GetAsync(cancellationToken: cancellationToken)
                .ReceiveJson<AuthentikPageResult<TModel>>();

            return results.Results.FirstOrDefault();
        }
        catch (FlurlHttpException e) { throw await WrapAsync(e, "GET", request.Url); }
    }

    public async Task<TModel?> FindByNameAsync(string name, IdPProvider provider, CancellationToken cancellationToken)
    => await FindAsync(name, null, provider, cancellationToken);

    public Task<TModel> CreateAsync(TModelWrite model, IdPProvider provider, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<TModel> UpdateAsync(TId id, TModelWrite model, IdPProvider provider, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(TId id, IdPProvider provider, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task<IFlurlRequest> BuildUrl(IdPProvider provider, string relativePath, CancellationToken cancellationToken)
    {
        var token = await authService.AcquireTokenAsync(provider, cancellationToken);

        return provider.BaseUrl
            .WithOAuthBearerToken(token)
            .AppendPathSegments("api/v3")
            .AppendPathSegments(relativePath)
            .WithHeader("Accept", "application/json");
    }

    private static async Task<HttpException> WrapAsync(FlurlHttpException ex, string verb, string path)
    {
        var body = await ex.GetResponseStringAsync();
        return new HttpException(body, null, verb, path, ex.StatusCode);
    }
}