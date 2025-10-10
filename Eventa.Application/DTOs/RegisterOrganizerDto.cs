namespace Eventa.Application.DTOs
{
    public class RegisterOrganizerDto
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Organization { get; set; }
    }
}
