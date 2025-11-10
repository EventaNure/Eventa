using Avalonia.Controls;
using Eventa.ViewModels.Events;

namespace Eventa.Views.Events;

public partial class ViewEventView : UserControl
{
    private static ViewEventView? _instance;
    public static ViewEventView Instance
    {
        get
        {
            _instance ??= new ViewEventView();
            return _instance;
        }
    }

    public readonly ViewEventViewModel viewEventViewModel = new();

    public ViewEventView()
    {
        DataContext = viewEventViewModel;
        InitializeComponent();
    }
}