namespace Moira.Common.Models;

public record IdPGroupStatus(string GroupId = "", string DisplayName = "", IEnumerable<string>? MemberOfGroupIds = null)
{
    public readonly IEnumerable<string> MemberOfGroupIds = MemberOfGroupIds ?? [];
}