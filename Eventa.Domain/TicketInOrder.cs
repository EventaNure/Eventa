namespace Eventa.Domain
{
    public class TicketInOrder
    {
        public int Id { get; set; }

        public int SeatId { get; set; }

        public Seat Seat { get; set; } = default!;

        public int OrderId { get; set; }

        public Order Order { get; set; } = default!;

        public double Price { get; set; }
    }
}
