using Moira.Common.Models;

namespace Moira.Common.Mappers;

public static class IdPGroupMappers
{
    public static IdPGroup CopyWithNewStatus(this IdPGroup idPGroup, IdPGroupStatus status)
    {
        return idPGroup with { Status = status };
    }
}