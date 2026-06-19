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
    UnsupportedProvider,
    UnknownError
}
