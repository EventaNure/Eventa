using Avalonia.Controls;
using Eventa.ViewModels.Tickets;

namespace Eventa.Views.Tickets;

public partial class ViewPurchasedTicketsView : UserControl
{
    private static ViewPurchasedTicketsView? _instance;
    public static ViewPurchasedTicketsView Instance
    {
        get
        {
            _instance ??= new ViewPurchasedTicketsView();
            return _instance;
        }
    }

    public readonly ViewPurchasedTicketsViewModel viewPurchasedTicketsViewModel = new();

    public ViewPurchasedTicketsView()
    {
        DataContext = viewPurchasedTicketsViewModel;
        InitializeComponent();
    }
}