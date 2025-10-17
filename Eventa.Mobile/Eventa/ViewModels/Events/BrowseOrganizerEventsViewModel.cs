using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Events.Organizer;
using Eventa.Services;
using Eventa.Views.Events;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Events;

public partial class BrowseOrganizerEventsViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    
    [ObservableProperty]
    private ObservableCollection<OrganizerEventResponseModel> _events = [];

    [ObservableProperty]
    private AsyncRelayCommand<OrganizerEventResponseModel> _editEventCommand;

    [ObservableProperty]
    private bool _isCompact = true;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isCreating;

    [ObservableProperty]
    private UserControl _createPage;

    [ObservableProperty]
    private bool _noEvents;

    public BrowseOrganizerEventsViewModel()
    {
        _apiService = new ApiService();
        _createPage = CreateOrganizerEventView.Instance;
        _editEventCommand = new AsyncRelayCommand<OrganizerEventResponseModel>(EditEventAsync);
    }

    private async Task EditEventAsync(OrganizerEventResponseModel? eventToEdit)
    {
        if (eventToEdit == null)
            return;
    }

    [RelayCommand]
    private void CreateEvent()
    {
        IsCreating = true;
        CreateOrganizerEventView.Instance.createOrganizerEventViewModel.IsEditMode = false;
    }

    partial void OnNoEventsChanged(bool value)
    {
        if (value)
        {
            ErrorMessage = "There is no ongoing events. Expand and Create new events!";
        }
        else
        {
            ErrorMessage = string.Empty;
        }
    }
}