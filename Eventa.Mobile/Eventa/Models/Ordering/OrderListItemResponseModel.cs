using CommunityToolkit.Mvvm.ComponentModel;
using Eventa.Models.Comments;
using System;
using System.Collections.Generic;

namespace Eventa.Models.Ordering;

public partial class OrderListItemResponseModel : ObservableObject
{
    public int OrderId { get; set; }
    public int EventDateTimeId { get; set; }
    public DateTime EventDateTime { get; set; }
    public string EventName { get; set; } = string.Empty;
    public List<TicketResponseModel> Tickets { get; set; } = [];
    public double TotalCost { get; set; }
    public bool IsQrTokenUsed { get; set; }
    public CommentDataModel? Comment { get; set; }

    [ObservableProperty]
    private bool _canRate = false;
}