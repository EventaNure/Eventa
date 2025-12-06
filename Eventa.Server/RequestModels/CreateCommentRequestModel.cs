using System.ComponentModel.DataAnnotations;

namespace Eventa.Server.RequestModels
{
    public class CreateCommentRequestModel
    {
        public int OrderId { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        [StringLength(300, MinimumLength = 10)]
        public string? Content { get; set; }
    }
}
