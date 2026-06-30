namespace Moira.Common.Abstractions;

public interface IUpdateChecker<TDesired, TCurrent>
{
    bool ShouldUpdate(TDesired desired, TCurrent current);
}
