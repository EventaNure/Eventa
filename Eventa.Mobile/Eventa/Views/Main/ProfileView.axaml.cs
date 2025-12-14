using Avalonia.Controls;
using Eventa.ViewModels.Main;

namespace Eventa.Views.Main;

public partial class ProfileView : UserControl
{
    private static ProfileView? _instance;
    public static ProfileView Instance
    {
        get
        {
            _instance ??= new ProfileView();
            return _instance;
        }
    }

    public readonly ProfileViewModel profileViewModel = new();

    public ProfileView()
    {
        DataContext = profileViewModel;
        InitializeComponent();
    }
}