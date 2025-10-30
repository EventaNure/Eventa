namespace Eventa.Domain
{
    public class Seat
    {
        public int Id { get; set; }

        public int SeatNumber { get; set; }

        public double PriceMultiplier { get; set; }

        public int RowId { get; set; }

        public Row Row { get; set; } = default!;

        public Cart[] Carts { get; set; } = [];

        public Order[] Orders { get; set; } = [];
    }
}
