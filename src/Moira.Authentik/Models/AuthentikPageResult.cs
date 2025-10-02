namespace Moira.Authentik.Models;

public sealed record AuthentikPageResult<TModel>(
    int Count,
    string? Next,
    string? Previous,
    IReadOnlyList<TModel> Results);