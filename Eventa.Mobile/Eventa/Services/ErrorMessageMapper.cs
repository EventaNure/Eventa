using System.Collections.Generic;

namespace Eventa.Services;

public class ErrorMessageMapper
{
    private static readonly Dictionary<string, string> ErrorMappings = new()
    {
        { "password must have at least one lowercase and one upperrcase letter", "Password must have at least one lowercase and one uppercase letter!" },
        { "the field password must be a string with a minimum length of 8 and a maximum length of 128.", "Password must be at least 8 or maximum 128 characters!" },
        { "the field name must be a string with a minimum length of 3 and a maximum length of 32.", "Name must be at least 3 or maximum 32 characters!" },
        { "the field organization must be a string with a minimum length of 3 and a maximum length of 32.", "Organization Name must be at least 3 or maximum 32 characters!" },
        { "the field email must be a string with a minimum length of 5 and a maximum length of 254.", "Email must be at least 5 or maximum 254 characters!" },
        { "the email field is not a valid e-mail address.", "Email format is not correct! (email@example.com)" },
        { "usernotfound", "User not found!" },
        { "tokenincorrect", "The code is not correct!" },
        { "loginfailed", "Login or password is incorrect!" },
        { "registrationfailed", "Failed to register, try again!" },
        { "duplicateemail", "Email is already taken!" },
        { "emailalreadyconfirmed", "Email is already confirmed!" },
        { "smallnumberoftags", "The minimum amount of tags has is 3!" },
        { "datetimemustbeinthefuture", "The dates must be set to upcoming days!" },
        { "the field description must be a string with a minimum length of 300 and a maximum 3000", "Description must be at least 300 or maximum 3000 characters!" },
        { "invalidextension", "Only .jpg, .jpeg, .png, .webp images are allowed!" },
        { "the imagefile field is required", "You must upload image for event!" },
        { "seatforthiseventnotexist", "This seat is already booked by someone else!" }
    };

    public static string MapErrorMessage(string serverMessage)
    {
        if (string.IsNullOrWhiteSpace(serverMessage))
            return "Unknown error occurred!";

        var normalizedMessage = serverMessage.Trim().ToLowerInvariant();

        if (ErrorMappings.TryGetValue(normalizedMessage, out var exactMatch))
            return exactMatch;

        string? bestMatch = null;
        int longestMatchLength = 0;

        foreach (var kvp in ErrorMappings)
        {
            if (normalizedMessage.Contains(kvp.Key) && kvp.Key.Length > longestMatchLength)
            {
                bestMatch = kvp.Value;
                longestMatchLength = kvp.Key.Length;
            }
        }

        return bestMatch ?? serverMessage;
    }
}