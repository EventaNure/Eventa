using System.Collections.Generic;

namespace Eventa.Models.Seats;

public class RowTypeResponseModel
{
    public string Name { get; set; } = string.Empty;
    public List<RowResponseModel> Rows { get; set; } = new();
}