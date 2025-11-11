using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Eventa.ViewModels.Ordering;

public partial class RowViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private int _rowNumber;

    [ObservableProperty]
    private ObservableCollection<SeatViewModel> _seats = new();
}