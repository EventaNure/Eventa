using System.Text.Json.Serialization;

namespace Eventa.Models.Authentication;

public class LoginRequestModel
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}