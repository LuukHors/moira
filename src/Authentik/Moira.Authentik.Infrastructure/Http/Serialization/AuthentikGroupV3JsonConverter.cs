using System.Text.Json;
using System.Text.Json.Serialization;
using Moira.Authentik.Domain.Groups;

namespace Moira.Authentik.Infrastructure.Http.Serialization;

internal class AuthentikGroupV3JsonConverter : JsonConverter<AuthentikGroupV3>
{
    public override AuthentikGroupV3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var name = root.GetProperty("name").GetString() ?? string.Empty;
        var pk = root.TryGetProperty("pk", out var pkProp) ? pkProp.GetString() : null;

        IEnumerable<AuthentikUserV3>? usersObj = null;
        if (root.TryGetProperty("users_obj", out var usersObjProp) && usersObjProp.ValueKind == JsonValueKind.Array)
        {
            usersObj = usersObjProp.Deserialize<IEnumerable<AuthentikUserV3>>(options);
        }

        IReadOnlyDictionary<string, object> attributes = new Dictionary<string, object>();
        if (root.TryGetProperty("attributes", out var attrProp) && attrProp.ValueKind == JsonValueKind.Object)
        {
            attributes = attrProp.Deserialize<IReadOnlyDictionary<string, object>>(options) ?? attributes;
        }

        IEnumerable<string> roles = [];
        if (root.TryGetProperty("roles", out var rolesProp) && rolesProp.ValueKind == JsonValueKind.Array)
        {
            roles = rolesProp.Deserialize<IEnumerable<string>>(options) ?? roles;
        }

        // Handle both legacy "parent" (singular) and current "parents" (plural) field
        IEnumerable<string> parents = [];
        if (root.TryGetProperty("parents", out var parentsProp) && parentsProp.ValueKind == JsonValueKind.Array)
        {
            parents = parentsProp.Deserialize<IEnumerable<string>>(options) ?? parents;
        }
        else if (root.TryGetProperty("parent", out var parentProp) && parentProp.ValueKind == JsonValueKind.String)
        {
            var singleParent = parentProp.GetString();
            if (!string.IsNullOrEmpty(singleParent))
                parents = [singleParent];
        }

        return new AuthentikGroupV3(name, pk, usersObj, attributes, roles, parents);
    }

    public override void Write(Utf8JsonWriter writer, AuthentikGroupV3 value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, new JsonSerializerOptions(options) { Converters = { } });
    }
}
