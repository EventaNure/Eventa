
using Eventa.Services;
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

            // Handle array of error objects: [{"reasons":[],"message":"...","metadata":{"Code":"..."}}]
            if (root.ValueKind == JsonValueKind.Array)
            {
                var errorMessages = new List<string>();

                foreach (var errorItem in root.EnumerateArray())
                {
                    // Try to get metadata.Code first
                    if (errorItem.TryGetProperty("metadata", out var metadata) &&
                        metadata.TryGetProperty("Code", out var codeProperty))
                    {
                        var rawError = codeProperty.GetString();
                        if (!string.IsNullOrEmpty(rawError))
                        {
                            errorMessages.Add(ErrorMessageMapper.MapErrorMessage(rawError));
                        }
                    }
                    // Fallback to message property
                    else if (errorItem.TryGetProperty("message", out var messageProperty))
                    {
                        var message = messageProperty.GetString();
                        if (!string.IsNullOrEmpty(message))
                        {
                            errorMessages.Add(ErrorMessageMapper.MapErrorMessage(message));
                        }
                    }
                }

                if (errorMessages.Count != 0)
                {
                    return string.Join(" ", errorMessages);
                }
            }

            // Handle single error object with metadata: {"metadata":{"Code":"..."}}
            if (root.TryGetProperty("metadata", out var metadataObj) &&
                metadataObj.TryGetProperty("Code", out var codeProp))
            {
                var rawError = codeProp.GetString() ?? "An error occurred.";
                return ErrorMessageMapper.MapErrorMessage(rawError);
            }

            // Handle validation errors: {"errors":{"field":["error1","error2"]}}
            if (root.TryGetProperty("errors", out var errorsProperty))
            {
                var errorMessages = new List<string>();

                foreach (var errorField in errorsProperty.EnumerateObject())
                {
                    if (errorField.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var error in errorField.Value.EnumerateArray())
                        {
                            var errorText = error.GetString();
                            if (!string.IsNullOrEmpty(errorText))
                            {
                                var mappedError = ErrorMessageMapper.MapErrorMessage(errorText);
                                errorMessages.Add(mappedError);
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
            var fallbackMessage = errorContent.Length > 200
                ? string.Concat(errorContent.AsSpan(0, 200), "...")
                : errorContent;

            return ErrorMessageMapper.MapErrorMessage(fallbackMessage);
        }
    }

    public static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        return ExtractErrorMessage(errorContent);
    }
}