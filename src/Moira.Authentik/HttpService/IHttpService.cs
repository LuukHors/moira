using Moira.Authentik.Models;
using Moira.Common.Models;

namespace Moira.Authentik.HttpService;

public interface IHttpService<TModel, in TModelWrite, in TId>
{
    Task<AuthentikPageResult<TModel>> ListAsync(string? name, IReadOnlyDictionary<string, object>? attributes, IdPProvider provider, TId? id = default, CancellationToken cancellationToken = default);
    Task<TModel?> GetByNameAsync(string name, IdPProvider provider, IReadOnlyDictionary<string, object>? attributes, CancellationToken cancellationToken);
    Task<TModel?> GetByIdAsync(TId id, IdPProvider provider, IReadOnlyDictionary<string, object>? attributes, CancellationToken cancellationToken);
    Task<TModel> CreateAsync(TModelWrite model, IdPProvider provider, CancellationToken cancellationToken);
    Task<TModel> UpdateAsync(TId id, TModelWrite model, IdPProvider provider, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(TId id, IdPProvider provider, CancellationToken cancellationToken);
}