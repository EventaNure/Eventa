using System;
using System.Collections.Generic;

namespace Eventa.Models.Ordering;

public class OrderResponseModel
{
    public int OrderId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public int EventDateTimeId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public List<TicketResponseModel> Tickets { get; set; } = new();
    public TimeSpan ExpireAt { get; set; }
    public double TotalPrice { get; set; }
}