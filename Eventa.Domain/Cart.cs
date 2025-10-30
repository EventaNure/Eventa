namespace Eventa.Domain
{
    public class Cart
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int SeatId { get; set; }

        public Seat Seat { get; set; } = default!;

        public int EventId { get; set; }

        public Event Event { get; set; } = default!;
    }
}
