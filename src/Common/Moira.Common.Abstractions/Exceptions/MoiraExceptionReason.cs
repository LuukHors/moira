namespace Moira.Common.Abstractions.Exceptions;

public enum MoiraExceptionReason
{
    IdpError,
    IdpRequestFailed,
    IdpValidationFailed,
    SecretMissing,
    SecretKeyMissing,
    ProviderMissing,
    ProviderAdapterMissing,
    ValidationFailed,
    UnsupportedProvider,
    UnknownError
}
