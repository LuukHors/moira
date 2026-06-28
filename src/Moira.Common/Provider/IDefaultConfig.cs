namespace Moira.Common.Provider;

public interface IDefaultConfig<out TEntity>
{
    TEntity Receive();
}