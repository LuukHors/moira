using Moira.Authentik.Application.Models.Group;
using Moira.Authentik.Domain.Groups;

namespace Moira.Authentik.Application.Mappers;

public static class GroupMapper
{
    public static AuthentikGroupV3 ToAuthentikGroup(this AuthentikGroupModel model, IEnumerable<string>? parentGroupIds, IEnumerable<string>? roleIds, IReadOnlyDictionary<string, object>? attributes = null)
    {
        return new AuthentikGroupV3(
            model.Spec.DisplayName,
            model.Status.GroupId,
            [],
            attributes ?? new Dictionary<string, object>(),
            roleIds ?? [],
            parentGroupIds ?? []
        );
    }
}
