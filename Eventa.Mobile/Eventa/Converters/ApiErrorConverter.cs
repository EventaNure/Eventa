using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eventa.Converters;

public class ApiErrorConverter
{
    public static string ExtractErrorMessage(string errorContent)
    {
        if (string.IsNullOrWhiteSpace(errorContent))
            return "An unknown error occurred.";

        try
        {
            var jsonDoc = JsonDocument.Parse(errorContent);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("metadata", out var metadata) &&
                metadata.TryGetProperty("Code", out var codeProperty))
            {
                return codeProperty.GetString() ?? "An error occurred.";
            }

            if (root.TryGetProperty("errors", out var errorsProperty))
            {
                var errorMessages = new List<string>();

                foreach (var errorField in errorsProperty.EnumerateObject())
                {
                    var fieldName = errorField.Name;

                    if (errorField.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var error in errorField.Value.EnumerateArray())
                        {
                            var errorText = error.GetString();
                            if (!string.IsNullOrEmpty(errorText))
                            {
                                errorMessages.Add(errorText);
                            }
                        }
                    }
                }

                if (errorMessages.Count != 0)
                {
                    return string.Join(" ", errorMessages);
                }
            }

            return "An error occurred while processing your request.";
        }
        catch (JsonException)
        {
            return errorContent.Length > 200
                ? string.Concat(errorContent.AsSpan(0, 200), "...")
                : errorContent;
        }
    }

    public static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        return ExtractErrorMessage(errorContent);
    }
}