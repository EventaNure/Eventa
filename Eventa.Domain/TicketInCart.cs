namespace Eventa.Domain
{
    public class TicketInCart
    {
        public string UserId { get; set; } = string.Empty;

        public int SeatId { get; set; }

        public Seat Seat { get; set; } = default!;

        public double Price { get; set; }
    }
}
