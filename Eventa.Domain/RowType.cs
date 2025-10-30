namespace Eventa.Domain
{
    public class RowType
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsSeparatedSeats { get; set; }

        public int PlaceId { get; set; }

        public Place Place { get; set; } = default!;

        public ICollection<Row> Rows { get; set; } = [];
    }
}
