namespace Moira.Authentik.HttpService;

public interface IAuthentikRoute<TModel, TModelWrite, in TId>
{
    string CollectionEntityPath { get; }
    string SingleEntityPath(TId id);
}