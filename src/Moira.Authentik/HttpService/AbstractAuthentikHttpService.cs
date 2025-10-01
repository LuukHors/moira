using Flurl;
using Flurl.Http;
using Moira.Authentik.Authentication;
using Moira.Common.Models;

namespace Moira.Authentik.HttpService;

public abstract class AbstractAuthentikHttpService<TModel, TModelWrite>(
    IAuthentikAuthenticationService authService) : IHttpService<TModel, TModelWrite>
{
    protected abstract string BasePath { get; }
    protected abstract string BasePathWithIdentifier { get; }


    private Url ParseBaseUrl(string url, string? identifier = null)
    {
        var result = new Url(url);
        return $"{result.Root}/{BasePathWithIdentifier}".Replace("{id}", identifier);
    }

    public Task<TModel?> GetAsync(string id, IDictionary<string, object> options, IdPProvider provider, CancellationToken cancellationToken) =>
        ParseBaseUrl(provider.BaseUrl)
            .AppendPathSegment(BasePath)
            .SetQueryParams(options)
            .GetJsonAsync<TModel?>(cancellationToken: cancellationToken);

    public Task<IEnumerable<TModel>> GetAllAsync(IDictionary<string, object> options, IdPProvider provider, CancellationToken cancellationToken) => throw new NotImplementedException();
    
    public Task<TModel> CreateAsync(TModelWrite model, IDictionary<string, object> options, IdPProvider provider, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<TModel> UpdateAsync(string id, TModelWrite model, IDictionary<string, object> options, IdPProvider provider, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<bool> DeleteAsync(string id, IDictionary<string, object> options, IdPProvider provider, CancellationToken cancellationToken) => throw new NotImplementedException();
}