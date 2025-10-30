namespace Eventa.Application.DTOs.Sections
{
    public class RowDto
    {
        public int Id { get; set; }

        public int RowNumber { get; set; }

        public SeatDto[] Seats { get; set; } = [];
    }
}
