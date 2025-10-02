namespace Moira.Authentik.HttpService;

public interface IAuthentikRoute<TModel, TModelWrite, in TId>
{
    string CollectionPath { get; }
    string SinglePath(TId id);
}