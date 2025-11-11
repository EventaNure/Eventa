using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Eventa.Controls;
using Eventa.ViewModels.Ordering;

namespace Eventa.Views.Ordering;

public partial class SeatOrderView : UserControl
{
    private static SeatOrderView? _instance;
    public static SeatOrderView Instance
    {
        get
        {
            _instance ??= new SeatOrderView();
            return _instance;
        }
    }

    public readonly SeatOrderViewModel seatOrderViewModel = new();

    public SeatOrderView()
    {
        DataContext = seatOrderViewModel;
        InitializeComponent();
    }

    private void OnImageClick(object? sender, PointerPressedEventArgs e)
    {
        ZoomImageDialog.Instance.Show(seatOrderViewModel.HallPlanUrl);
    }
}