using System.Text.Json.Serialization;

namespace Eventa.Models.Authentication;

public class ResendEmailConfirmationRequestModel
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
}