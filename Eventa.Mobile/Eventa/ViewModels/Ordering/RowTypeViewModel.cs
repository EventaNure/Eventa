using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Eventa.ViewModels.Ordering;

public partial class RowTypeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private IBrush _color = Brushes.Gray;

    [ObservableProperty]
    private string _priceRange = "[price]";

    [ObservableProperty]
    private ObservableCollection<RowViewModel> _rows = new();
}