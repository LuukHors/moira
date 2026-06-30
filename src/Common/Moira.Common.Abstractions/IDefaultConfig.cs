namespace Moira.Common.Abstractions;

public interface IDefaultConfig<out TEntity>
{
    TEntity Receive();
}