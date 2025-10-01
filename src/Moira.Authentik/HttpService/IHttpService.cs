using Moira.Common.Models;

namespace Moira.Authentik.HttpService;

public interface IHttpService<TModel, in TModelWrite>
{
    Task<TModel?> GetAsync(object options, IdPProvider provider, CancellationToken cancellationToken);
    Task<TModel> CreateAsync(TModelWrite model, IdPProvider provider, CancellationToken cancellationToken);
    Task<TModel> UpdateAsync(string id, TModelWrite model, IdPProvider provider, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, IdPProvider provider, CancellationToken cancellationToken);
}