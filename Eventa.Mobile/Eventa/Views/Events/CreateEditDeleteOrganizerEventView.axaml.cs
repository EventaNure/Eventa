using Avalonia.Controls;
using Eventa.ViewModels.Events;

namespace Eventa.Views.Events;

public partial class CreateEditDeleteOrganizerEventView : UserControl
{
    private static CreateEditDeleteOrganizerEventView? _instance;
    public static CreateEditDeleteOrganizerEventView Instance
    {
        get
        {
            _instance ??= new CreateEditDeleteOrganizerEventView();
            return _instance;
        }
    }

    public readonly CreateEditDeleteOrganizerEventViewModel createEditDeleteOrganizerEventViewModel = new();

    public CreateEditDeleteOrganizerEventView()
    {
        DataContext = createEditDeleteOrganizerEventViewModel;
        InitializeComponent();
    }
}