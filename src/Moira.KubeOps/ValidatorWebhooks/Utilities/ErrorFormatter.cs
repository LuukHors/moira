using FluentValidation;
using System.Text;

namespace Moira.KubeOps.ValidatorWebhooks.Utilities;

internal static class ErrorFormatter
{
    public static string FormatError(this ValidationException ex)
    {
        var stringbuilder = new StringBuilder("One or more errors occured: ");

        foreach (var error in ex.Errors)
        {
            stringbuilder.Append(error.PropertyName);
            stringbuilder.Append(": ");
            stringbuilder.Append(error.ErrorMessage);
            stringbuilder.Append(',');
        }

        stringbuilder.Remove(stringbuilder.Length - 1, 1); // remove trailing ','

        return stringbuilder.ToString();
    }
}