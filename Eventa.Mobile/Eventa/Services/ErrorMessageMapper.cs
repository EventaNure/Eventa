using System.Collections.Generic;

namespace Eventa.Services;

public class ErrorMessageMapper
{
    private static readonly Dictionary<string, string> ErrorMappings = new()
    {
        { "Password must have at least one lowercase and one upperrcase letter", "Password must have at least one lowercase and one uppercase letter!" },
        { "The field Password must be a string with a minimum length of 8 and a maximum length of 128.", "Password must be at least 8 or maximum 128 characters!" },
        { "The field Name must be a string with a minimum length of 3 and a maximum length of 32.", "Name must be at least 3 or maximum 32 characters!" },
        { "The field Organization must be a string with a minimum length of 3 and a maximum length of 32.", "Organization Name must be at least 3 or maximum 32 characters!" },
        { "The field Email must be a string with a minimum length of 5 and a maximum length of 254.", "Email must be at least 5 or maximum 254 characters!" },
        { "The Email field is not a valid e-mail address.", "Email format is not correct! (email@example.com)" },
        { "UserNotFound", "User not found!" },
        { "TokenIncorrect", "The code is not correct!" },
        { "LoginFailed", "Login or password is incorrect!" },
        { "RegistrationFailed", "Failed to register, try again!" },
        { "DuplicateEmail", "Email is already taken!" },
        { "EmailAlreadyConfirmed", "Email is already confirmed!" },
    };

    public static string MapErrorMessage(string serverMessage)
    {
        if (string.IsNullOrWhiteSpace(serverMessage))
            return "Unknown error occured!";

        var normalizedMessage = serverMessage.Trim().ToLowerInvariant();

        if (ErrorMappings.TryGetValue(normalizedMessage, out var exactMatch))
            return exactMatch;

        string? bestMatch = null;
        int longestMatchLength = 0;

        foreach (var kvp in ErrorMappings)
        {
            var key = kvp.Key.ToLowerInvariant();

            if (normalizedMessage.Contains(key) && key.Length > longestMatchLength)
            {
                bestMatch = kvp.Value;
                longestMatchLength = key.Length;
            }
        }

        return bestMatch ?? serverMessage;
    }
}