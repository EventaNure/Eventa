namespace Eventa.Application.DTOs.Comments
{
    public class CommentDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Content { get; set; }
        public DateTime CreationDateTime { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
