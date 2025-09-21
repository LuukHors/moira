using Moira.Common.Models;

namespace Moira.Authentik.HttpService;

public interface IAuthentikHttpService<TEntity, TAuthentikEntity> where TEntity : IdPEntity
{
    public Task<TAuthentikEntity?> GetAsync(TEntity entity, CancellationToken cancellationToken);    
    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken);
    public Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken);
}