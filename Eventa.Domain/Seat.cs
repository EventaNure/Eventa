namespace Eventa.Domain
{
    public class Seat
    {
        public int Id { get; set; }

        public int SeatNumber { get; set; }

        public double PriceMultiplier { get; set; }

        public int RowId { get; set; }

        public Row Row { get; set; } = default!;

        public ICollection<TicketInCart> TicketsInCart { get; set; } = [];

        public ICollection<TicketInOrder> TicketsInOrder { get; set; } = [];
    }
}
