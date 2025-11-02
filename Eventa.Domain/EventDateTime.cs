namespace Eventa.Domain
{
    public class EventDateTime
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public Event Event { get; set; } = default!;

        public DateTime StartDateTime { get; set; }

        public ICollection<TicketInCart> TicketsInCart { get; set; } = [];

        public ICollection<Order> Orders { get; set; } = [];
    }
}
