namespace Moira.Common.Abstractions.Models;

public record ResourceRef(string Kind, string Name, string Namespace = "default", string ApiVersion = "moira.operator/v1alpha1");
