using Avalonia.Controls;
using Eventa.ViewModels.Ordering;

namespace Eventa.Views.Ordering;

public partial class TicketOrderView : UserControl
{
    private static TicketOrderView? _instance;
    public static TicketOrderView Instance
    {
        get
        {
            _instance ??= new TicketOrderView();
            return _instance;
        }
    }

    public readonly TicketOrderViewModel ticketOrderViewModel = new();

    public TicketOrderView()
    {
        DataContext = ticketOrderViewModel;
        InitializeComponent();
    }
}