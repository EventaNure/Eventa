using Eventa.Application.DTOs.Comments;
using Eventa.Application.DTOs.TicketInCarts;

namespace Eventa.Application.DTOs.Orders
{
    public class OrderListItemDto
    {
        public int OrderId { get; set; }

        public int EventDateTimeId { get; set; }

        public DateTime EventDateTime { get; set; }

        public string EventName { get; set; } = string.Empty;

        public IEnumerable<TicketDto> Tickets { get; set; } = [];

        public double TotalCost { get; set; }

        public bool IsQrTokenUsed { get; set; }

        public CommentDto? Comment { get; set; }
    }
}
