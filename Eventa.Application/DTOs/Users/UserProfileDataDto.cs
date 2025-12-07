using Eventa.Application.DTOs.Comments;

namespace Eventa.Application.DTOs.Users
{
    public class UserProfileDataDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public double? Rating { get; set; }
        public string? Organization { get; set; }
        public IEnumerable<CommentDto> Comments { get; set; } = [];
    }
}
