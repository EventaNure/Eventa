using System.Text.Json.Serialization;

namespace Eventa.Models;

public class RegisterRequestModel
{
    [JsonPropertyName("name")]
    public string UserName { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
    
    [JsonPropertyName("confirmPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [JsonPropertyName("organizationName")]
    public string? OrganizationName { get; set; }
}