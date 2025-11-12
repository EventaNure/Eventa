using CommunityToolkit.Mvvm.ComponentModel;

namespace Eventa.ViewModels.Ordering;

public partial class OrderTicketViewModel : ObservableObject
{
    [ObservableProperty]
    private int _seatId;

    [ObservableProperty]
    private string _rowTypeName = string.Empty;

    [ObservableProperty]
    private int _rowNumber;

    [ObservableProperty]
    private int _seatNumber;

    [ObservableProperty]
    private double _price;
}