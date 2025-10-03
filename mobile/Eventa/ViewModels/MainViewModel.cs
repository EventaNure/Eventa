using CommunityToolkit.Mvvm.ComponentModel;

namespace Eventa.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _greeting = "Welcome to Eventa!";
}
