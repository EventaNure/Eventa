using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Events.Organizer;
using Eventa.Services;
using Eventa.Views.Events;
using Eventa.Views.Main;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Events;

public partial class BrowseOrganizerEventsViewModel : ObservableObject
{
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
        _createPage = CreateEditDeleteOrganizerEventView.Instance;
        _editEventCommand = new AsyncRelayCommand<OrganizerEventResponseModel>(EditEventAsync);
    }

    private async Task EditEventAsync(OrganizerEventResponseModel? eventToEdit)
    {
        if (eventToEdit == null)
            return;

        MainPageView.Instance.mainPageViewModel.OrganizerEventsClickedCommand.Execute(eventToEdit);
        IsCreating = true;
        await CreateEditDeleteOrganizerEventView.Instance.createEditDeleteOrganizerEventViewModel.LoadPlacesAndTagsAsync();
        CreateEditDeleteOrganizerEventView.Instance.createEditDeleteOrganizerEventViewModel.InsertFormData(MainPageView.Instance.mainPageViewModel.JwtToken, MainPageView.Instance.mainPageViewModel.UserId, eventToEdit.Id);
        CreateEditDeleteOrganizerEventView.Instance.createEditDeleteOrganizerEventViewModel.IsEditMode = true;
    }

    [RelayCommand]
    private async Task CreateEvent()
    {
        IsCreating = true;
        await CreateEditDeleteOrganizerEventView.Instance.createEditDeleteOrganizerEventViewModel.LoadPlacesAndTagsAsync();
        CreateEditDeleteOrganizerEventView.Instance.createEditDeleteOrganizerEventViewModel.InsertFormData(MainPageView.Instance.mainPageViewModel.JwtToken, MainPageView.Instance.mainPageViewModel.UserId);
        CreateEditDeleteOrganizerEventView.Instance.createEditDeleteOrganizerEventViewModel.IsEditMode = false;
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