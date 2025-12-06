namespace Eventa.Application.DTOs.Users
{
    public class ExternalLoginResultDto
    {
        public bool IsLogin { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
