using Moira.Authentik.Application.Models;
using Moira.Authentik.Domain.Groups;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Mappers;

public static class GroupMapper
{
    public static AuthentikGroupV3 ToAuthentikGroup(this AuthentikGroupModel model, IEnumerable<string>? parentGroupIds = null, IReadOnlyDictionary<string, object>? attributes = null)
    {
        return new AuthentikGroupV3(
            model.Spec.DisplayName,
            model.Status.GroupId,
            [],
            attributes ?? new Dictionary<string, object>(),
            [],
            parentGroupIds ?? []
        );
    }
}
