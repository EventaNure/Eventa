namespace Eventa.Application.DTOs.Comments
{
    public class CreateCommentDto
    {
        public int Rating { get; set; }
        public string? Content { get; set; }
        public int OrderId { get; set; }
    }
}
