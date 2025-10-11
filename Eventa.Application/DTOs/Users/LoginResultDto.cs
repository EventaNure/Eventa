namespace Eventa.Application.DTOs.Users
{
    public class LoginResultDto
    {
        public bool EmailConfirmed { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
