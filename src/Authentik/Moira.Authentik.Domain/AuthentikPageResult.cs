namespace Moira.Authentik.Domain;

public sealed record AuthentikPageResult<TModel>(
    int Count,
    string? Next,
    string? Previous,
    IEnumerable<TModel> Results);
