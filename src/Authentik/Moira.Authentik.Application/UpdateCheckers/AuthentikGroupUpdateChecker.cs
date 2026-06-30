using Moira.Authentik.Domain.Groups;
using Moira.Common.Abstractions;

namespace Moira.Authentik.Application.UpdateCheckers;

public class AuthentikGroupUpdateChecker : IUpdateChecker<AuthentikGroupV3, AuthentikGroupV3>
{
    public bool ShouldUpdate(AuthentikGroupV3 desired, AuthentikGroupV3 current)
    {
        if (!desired.name.Equals(current.name))
            return true;

        var desiredParentIds = desired.parents.ToHashSet();
        var currentParentIds = current.parents.ToHashSet();

        return !desiredParentIds.SetEquals(currentParentIds);
    }
}
