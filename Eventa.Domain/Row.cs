namespace Eventa.Domain
{
    public class Row
    {
        public int Id { get; set; }

        public int RowNumber { get; set; }

        public int RowTypeId { get; set; }

        public RowType RowType { get; set; } = default!;

        public ICollection<Seat> Seats { get; set; } = [];
    }
}
