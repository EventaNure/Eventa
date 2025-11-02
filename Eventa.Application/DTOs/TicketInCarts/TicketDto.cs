namespace Eventa.Application.DTOs.TicketInCarts
{
    public class TicketDto
    {
        public string RowTypeName { get; set; } = string.Empty;

        public int Row {  get; set; }

        public int SeatId { get; set; }

        public int SeatNumber { get; set; }

        public double Price { get; set; }
    }
}
