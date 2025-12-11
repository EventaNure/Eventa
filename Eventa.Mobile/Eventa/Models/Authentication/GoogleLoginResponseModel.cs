using System.Text.Json.Serialization;

namespace Eventa.Models.Authentication;

public class GoogleLoginResponseModel
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("jwtToken")]
    public string JwtToken { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("isLogin")]
    public bool IsLogin { get; set; }
}