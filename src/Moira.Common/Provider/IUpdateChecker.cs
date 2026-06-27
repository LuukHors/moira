namespace Moira.Common.Provider;

public interface IUpdateChecker<TDesired, TCurrent>
{
    bool ShouldUpdate(TDesired desired, TCurrent current);
}
