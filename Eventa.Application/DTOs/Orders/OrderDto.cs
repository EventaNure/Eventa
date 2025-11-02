using Eventa.Application.DTOs.TicketInCarts;

namespace Eventa.Application.DTOs.Orders
{
    public class OrderDto
    {
        public int OrderId { get; set; }

        public string SessionId { get; set; } = string.Empty;

        public int EventDateTimeId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public IEnumerable<TicketDto> Tickets { get; set; } = [];

        public TimeSpan ExpireAt { get; set; }

        public double TotalPrice { get; set; }
    }
}
