namespace Moira.Authentik.Infrastructure.Http.Routes;

internal interface IAuthentikRoute<TModel, TModelWrite, in TId>
{
    string CollectionEntityPath { get; }
    string SingleEntityPath(TId id);
}
