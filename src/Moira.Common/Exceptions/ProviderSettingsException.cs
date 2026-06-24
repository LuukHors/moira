namespace Moira.Common.Exceptions;

public class ProviderSettingsException(string message)
    : DependencyException(message, MoiraExceptionReason.UnsupportedProvider);
