namespace Moira.Authentik.Application.Models.Group;

public record AuthentikGroupStatus(string GroupId = "", string DisplayName = "", IEnumerable<string>? MemberOfGroupIds = null)
{
    public readonly IEnumerable<string> MemberOfGroupIds = MemberOfGroupIds ?? [];
}