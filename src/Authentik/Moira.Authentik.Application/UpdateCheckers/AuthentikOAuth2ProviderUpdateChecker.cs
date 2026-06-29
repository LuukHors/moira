using System.Text.Json;
using Moira.Authentik.Domain.Applications;
using Moira.Common.Provider;

namespace Moira.Authentik.Application.UpdateCheckers;

public class AuthentikOAuth2ProviderUpdateChecker : IUpdateChecker<AuthentikOAuth2ProviderV3, AuthentikOAuth2ProviderV3>
{
    public bool ShouldUpdate(AuthentikOAuth2ProviderV3 desired, AuthentikOAuth2ProviderV3 current)
    {
        if (!string.Equals(desired.name, current.name, StringComparison.Ordinal))
            return true;

        if (!string.Equals(desired.client_id, current.client_id, StringComparison.Ordinal))
            return true;

        if (!string.Equals(desired.client_type, current.client_type, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!string.Equals(desired.authorization_flow, current.authorization_flow, StringComparison.Ordinal))
            return true;

        if (!string.Equals(desired.invalidation_flow, current.invalidation_flow, StringComparison.Ordinal))
            return true;

        if (!string.Equals(
                NormalizeOptionalText(desired.logout_uri),
                NormalizeOptionalText(current.logout_uri),
                StringComparison.Ordinal))
            return true;

        var desiredPropertyMappings = ToAuthentikReferenceIdSet(desired.property_mappings);
        var currentPropertyMappings = ToAuthentikReferenceIdSet(current.property_mappings);
        if (!desiredPropertyMappings.SetEquals(currentPropertyMappings))
            return true;

        var desiredRedirectUris = (desired.redirect_uris ?? [])
            .Select(uri => (
                NormalizeRequiredText(uri.matching_mode).ToUpperInvariant(),
                NormalizeRequiredText(uri.url),
                NormalizeRedirectUriType(uri.redirect_uri_type)))
            .ToHashSet();
        var currentRedirectUris = (current.redirect_uris ?? [])
            .Select(uri => (
                NormalizeRequiredText(uri.matching_mode).ToUpperInvariant(),
                NormalizeRequiredText(uri.url),
                NormalizeRedirectUriType(uri.redirect_uri_type)))
            .ToHashSet();

        if (!desiredRedirectUris.SetEquals(currentRedirectUris))
            return true;

        if (!string.Equals(NormalizeOptionalText(desired.access_code_validity), NormalizeOptionalText(current.access_code_validity), StringComparison.Ordinal))
            return true;

        if (!string.Equals(NormalizeOptionalText(desired.access_token_validity), NormalizeOptionalText(current.access_token_validity), StringComparison.Ordinal))
            return true;

        if (!string.Equals(NormalizeOptionalText(desired.refresh_token_validity), NormalizeOptionalText(current.refresh_token_validity), StringComparison.Ordinal))
            return true;

        return false;
    }

    private static ISet<string> ToAuthentikReferenceIdSet(IEnumerable<object>? values)
    {
        return (values ?? [])
            .Select(NormalizeAuthentikReferenceId)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeAuthentikReferenceId(object? value)
    {
        return value switch
        {
            null => string.Empty,
            string text => text,
            int number => number.ToString(),
            long number => number.ToString(),
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString() ?? string.Empty,
            JsonElement { ValueKind: JsonValueKind.Number } element => element.GetRawText(),
            JsonElement { ValueKind: JsonValueKind.Object } element when element.TryGetProperty("pk", out var pk) =>
                NormalizeAuthentikReferenceId(pk),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
    }

    private static string NormalizeRequiredText(string? value)
    {
        return value ?? string.Empty;
    }

    private static string NormalizeRedirectUriType(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "AUTHORIZATION" : value.ToUpperInvariant();
    }
}
