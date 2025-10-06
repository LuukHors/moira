using System.Collections.ObjectModel;
using Moira.Authentik.Models.V3;
using Moira.Common.Models;

namespace Moira.Authentik.Models.Mappers;

public static class GroupMapper
{
    public static AuthentikGroupV3 ToAuthentikGroup(this IdPGroup model, string parentGroupId = "", IReadOnlyDictionary<string, object>? attributes = null)
    {
        return new AuthentikGroupV3(
            model.Spec.DisplayName,
            model.Status.GroupId,
            [],
            attributes ?? new Dictionary<string, object>(),
            [],
            parentGroupId
        );
    }
}