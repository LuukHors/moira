namespace Moira.Common.Abstractions.Exceptions;

public class ProviderSettingsException(string message)
    : DependencyException(message, MoiraExceptionReason.UnsupportedProvider);
