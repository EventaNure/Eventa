using Eventa.Models.Ordering;
using System;
using System.Collections.Generic;

namespace Eventa.Models.Carting;

public class CartResponseModel
{
    public int EventDateTimeId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public List<TicketResponseModel> Tickets { get; set; } = [];
    public TimeSpan ExpireAt { get; set; }
    public double TotalCost { get; set; }
}