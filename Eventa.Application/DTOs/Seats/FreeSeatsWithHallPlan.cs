using Eventa.Application.DTOs.Sections;

namespace Eventa.Application.DTOs.Seats
{
    public class FreeSeatsWithHallPlan
    {
        public IEnumerable<RowTypeDto> RowTypes { get; set; } = [];

        public string HallPlanUrl { get; set; } = string.Empty;
    }
}
