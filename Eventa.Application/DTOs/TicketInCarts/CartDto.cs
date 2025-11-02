namespace Eventa.Application.DTOs.TicketInCarts
{
    public class CartDto
    {
        public int EventDateTimeId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public IEnumerable<TicketDto> Tickets { get; set; } = [];

        public TimeSpan ExpireAt {  get; set; }

        public double TotalCost { get; set; }
    }
}
