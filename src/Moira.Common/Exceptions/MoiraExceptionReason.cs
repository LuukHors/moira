namespace Moira.Common.Exceptions;

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
