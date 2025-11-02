namespace Eventa.Application.DTOs.TicketInCarts
{
    public class CartDataDto
    {
        public int EventDateTimeId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public IEnumerable<TicketDto> Tickets { get; set; } = [];

        public DateTime ExpireDateTime { get; set; }

        public double TotalCost { get; set; }
    }
}
