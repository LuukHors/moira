namespace Moira.Common.Abstractions;

public interface IUpdateChecker<in TDesired, in TCurrent>
{
    bool ShouldUpdate(TDesired desired, TCurrent current);
}
