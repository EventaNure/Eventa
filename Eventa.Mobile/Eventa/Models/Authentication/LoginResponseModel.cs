using System.Text.Json.Serialization;

namespace Eventa.Models.Authentication;

public class LoginResponseModel
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("jwtToken")]
    public string JwtToken { get; set; } = string.Empty;

    [JsonPropertyName("emailConfirmed")]
    public bool EmailConfirmed { get; set; }
}