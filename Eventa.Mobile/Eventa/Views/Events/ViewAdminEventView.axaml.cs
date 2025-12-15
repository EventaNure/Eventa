using Avalonia.Controls;
using Eventa.ViewModels.Events;

namespace Eventa.Views.Events;

public partial class ViewAdminEventView : UserControl
{
    private static ViewAdminEventView? _instance;
    public static ViewAdminEventView Instance
    {
        get
        {
            _instance ??= new ViewAdminEventView();
            return _instance;
        }
    }

    public readonly ViewAdminEventViewModel viewAdminEventViewModel = new();

    public ViewAdminEventView()
    {
        DataContext = viewAdminEventViewModel;
        InitializeComponent();
    }
}