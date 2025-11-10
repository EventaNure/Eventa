using System.Collections.Generic;

namespace Eventa.Models.Seats;

public class FreeSeatsWithHallPlanResponseModel
{
    public List<RowTypeResponseModel> RowTypes { get; set; } = new();
    public string HallPlanUrl { get; set; } = string.Empty;
}