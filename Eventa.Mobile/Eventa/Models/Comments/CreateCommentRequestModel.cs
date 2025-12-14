namespace Eventa.Models.Comments;

public class CreateCommentRequestModel
{
    public int OrderId { get; set; }
    public int Rating { get; set; }
    public string? Content { get; set; }
}