namespace Moira.Authentik.Application.Models;

public record AuthentikGroupStatus(string GroupId = "", string DisplayName = "", IEnumerable<string>? MemberOfGroupIds = null)
{
    public readonly IEnumerable<string> MemberOfGroupIds = MemberOfGroupIds ?? [];
}