using System.Collections.Generic;

namespace Eventa.Models.Seats;

public class RowResponseModel
{
    public int Id { get; set; }
    public int RowNumber { get; set; }
    public List<SeatResponseModel> Seats { get; set; } = [];
}