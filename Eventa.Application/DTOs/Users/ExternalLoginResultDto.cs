namespace Eventa.Application.DTOs.Users
{
    public class ExternalLoginResultDto
    {
        public string UserId { get; set; } = string.Empty;

        public string? Role { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
