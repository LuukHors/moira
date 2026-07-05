using Moira.Authentik.Domain.Applications;
using Moira.Authentik.Domain.Roles;
using Moira.Common.Abstractions;

namespace Moira.Authentik.Application.UpdateCheckers;

public class AuthentikRoleUpdateChecker : IUpdateChecker<AuthentikRoleV3, AuthentikRoleV3>
{
    public bool ShouldUpdate(AuthentikRoleV3 desired, AuthentikRoleV3 current)
    {
        return desired.name != current.name;
    }
}
