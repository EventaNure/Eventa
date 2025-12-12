using System.Text.Json.Serialization;

namespace Eventa.Models.Authentication;

public class CompleteExternalRegistrationRequestModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("organization")]
    public string? Organization { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
}