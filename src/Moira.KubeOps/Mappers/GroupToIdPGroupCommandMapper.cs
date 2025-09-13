using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Mappers;

internal static class GroupToIdPGroupCommandMapper
{
    public static IdPCommand<IdPGroup> ToCommand(this Group group)
    {
        return new IdPCommand<IdPGroup>(
            Guid.NewGuid(),
            new IdPGroup("test", "groupk8sname", "groupdisplayname", [])
        );
    }
}
