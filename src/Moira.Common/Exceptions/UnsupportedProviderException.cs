namespace Moira.Common.Exceptions;

public class UnsupportedProviderException(string entityType)
    : DependencyException($"Unable to determine provider type for entity {entityType}", MoiraExceptionReason.UnsupportedProvider)
{
    public string EntityType { get; } = entityType;
}
