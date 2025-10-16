namespace Moira.Common.RequestContext;

public interface IRequestContextProvider
{
    public Guid RequestId { get; }
}