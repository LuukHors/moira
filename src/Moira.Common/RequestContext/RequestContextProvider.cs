namespace Moira.Common.RequestContext;

public class RequestContextProvider : IRequestContextProvider
{
    public Guid RequestId { get; } = Guid.NewGuid();
}