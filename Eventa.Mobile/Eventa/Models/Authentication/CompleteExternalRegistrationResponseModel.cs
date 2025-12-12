using System.Text.Json.Serialization;

namespace Eventa.Models.Authentication;

public class CompleteExternalRegistrationResponseModel
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("jwtToken")]
    public string JwtToken { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}