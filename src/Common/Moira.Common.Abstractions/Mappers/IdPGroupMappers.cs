using Moira.Common.Abstractions.Models;

namespace Moira.Common.Abstractions.Mappers;

public static class IdPGroupMappers
{
    public static IdPGroup CopyWithNewStatus(this IdPGroup idPGroup, IdPGroupStatus status)
    {
        return idPGroup with { Status = status };
    }
}