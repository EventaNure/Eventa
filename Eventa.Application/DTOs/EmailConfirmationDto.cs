namespace Eventa.Application.DTOs
{
    public class EmailConfirmationDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}