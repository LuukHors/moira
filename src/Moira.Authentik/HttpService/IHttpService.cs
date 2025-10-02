using Moira.Common.Models;

namespace Moira.Authentik.HttpService;

public interface IHttpService<TModel, in TModelWrite, in TId>
{
    Task<TModel?> GetAsync(object options, IdPProvider provider, CancellationToken cancellationToken);
    Task<TModel> CreateAsync(TModelWrite model, IdPProvider provider, CancellationToken cancellationToken);
    Task<TModel> UpdateAsync(TId id, TModelWrite model, IdPProvider provider, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(TId id, IdPProvider provider, CancellationToken cancellationToken);
}