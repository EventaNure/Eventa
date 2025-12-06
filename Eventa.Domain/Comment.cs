using System.ComponentModel.DataAnnotations;

namespace Eventa.Domain
{
    public class Comment
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public Order Order { get; set; } = default!;
    }
}
