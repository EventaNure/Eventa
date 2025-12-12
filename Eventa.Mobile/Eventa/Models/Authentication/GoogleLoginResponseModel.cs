using System.Text.Json.Serialization;

namespace Eventa.Models.Authentication;

public class GoogleLoginResponseModel
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("jwtToken")]
    public string? JwtToken { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonIgnore]
    public bool IsLogin => !string.IsNullOrEmpty(JwtToken) && !string.IsNullOrEmpty(Role);
}