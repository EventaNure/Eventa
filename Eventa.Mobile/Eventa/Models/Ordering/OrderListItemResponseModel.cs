using System.Collections.Generic;

namespace Eventa.Models.Ordering;

public class OrderListItemResponseModel
{
    public int OrderId { get; set; }
    public int EventDateTimeId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public List<TicketResponseModel> Tickets { get; set; } = new();
    public double TotalCost { get; set; }
}
