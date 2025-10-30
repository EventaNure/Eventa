using Eventa.Application.DTOs.Sections;

namespace Eventa.Application.DTOs.Seats
{
    public class GetFreeSeatsResultDto
    {
        public IEnumerable<RowTypeDto> RowTypes { get; set; } = [];

        public int PlaceId { get; set; }
    }
}
