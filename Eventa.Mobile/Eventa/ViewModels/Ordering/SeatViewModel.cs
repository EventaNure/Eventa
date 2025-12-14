using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Eventa.ViewModels.Ordering;

public partial class SeatViewModel : ObservableObject
{
    private readonly Color _baseColor;

    public SeatViewModel(Color baseColor)
    {
        _baseColor = baseColor;
        UpdateColors();
    }

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private int _seatNumber;

    [ObservableProperty]
    private double _price;

    [ObservableProperty]
    private string _rowTypeName = string.Empty;

    [ObservableProperty]
    private int _rowNumber;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                UpdateColors();
            }
        }
    }

    [ObservableProperty]
    private Color _backgroundColor;

    [ObservableProperty]
    private Color _borderColor;

    public RelayCommand<SeatViewModel>? SelectSeatCommand { get; set; }

    private void UpdateColors()
    {
        if (_isSelected)
        {
            // Darker shade when selected
            BackgroundColor = Color.FromRgb(
                (byte)(_baseColor.R * 0.7),
                (byte)(_baseColor.G * 0.7),
                (byte)(_baseColor.B * 0.7)
            );
            BorderColor = Color.FromRgb(
                (byte)(_baseColor.R * 0.5),
                (byte)(_baseColor.G * 0.5),
                (byte)(_baseColor.B * 0.5)
            );
        }
        else
        {
            BackgroundColor = _baseColor;
            BorderColor = Color.FromRgb(
                (byte)(_baseColor.R * 0.85),
                (byte)(_baseColor.G * 0.85),
                (byte)(_baseColor.B * 0.85)
            );
        }
    }


}
