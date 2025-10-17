using Avalonia.Controls;
using Eventa.ViewModels.Events;

namespace Eventa.Views.Events;

public partial class CreateOrganizerEventView : UserControl
{
    private static CreateOrganizerEventView? _instance;
    public static CreateOrganizerEventView Instance
    {
        get
        {
            _instance ??= new CreateOrganizerEventView();
            return _instance;
        }
    }

    public readonly CreateOrganizerEventViewModel createOrganizerEventViewModel = new();

    public CreateOrganizerEventView()
    {
        DataContext = createOrganizerEventViewModel;
        InitializeComponent();
    }
}