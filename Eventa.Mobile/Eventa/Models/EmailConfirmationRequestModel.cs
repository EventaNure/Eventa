using System.Text.Json.Serialization;

namespace Eventa.Models;

public class EmailConfirmationRequestModel
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}