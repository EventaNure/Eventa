using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Comments;
using Eventa.Services;
using Eventa.Views.Main;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Main;

public partial class ProfileViewModel : ObservableObject
{
    private readonly ApiService _apiService = new();

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _organization = string.Empty;

    [ObservableProperty]
    private double _rating = 0.0;

    [ObservableProperty]
    private bool _isOrganizer = false;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _showEventComments = true;

    [ObservableProperty]
    private bool _showMyComments = false;

    [ObservableProperty]
    private ObservableCollection<CommentDataModel> _eventComments = [];

    [ObservableProperty]
    private ObservableCollection<CommentDataModel> _myComments = [];

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // Rounded rating for star display (rounds to nearest integer)
    public int RoundedRating => (int)Math.Round(Rating);

    public async Task LoadProfileAsync(string jwtToken)
    {
        if (string.IsNullOrEmpty(jwtToken))
        {
            ErrorMessage = "You must be logged in to view your profile.";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var (success, message, userData) = await _apiService.GetPersonalDataAsync(jwtToken);

            if (success && userData != null)
            {
                UserName = userData.Name ?? "Username";
                Email = userData.Email ?? string.Empty;
                Organization = userData.Organization ?? string.Empty;
                Rating = userData.Rating ?? 0.0;
                IsOrganizer = !string.IsNullOrEmpty(userData.Organization);

                EventComments.Clear();
                MyComments.Clear();

                if (userData.CommentsAboutMe != null)
                {
                    foreach (var comment in userData.CommentsAboutMe)
                    {
                        EventComments.Add(comment);
                    }
                }

                if (userData.MyComments != null)
                {
                    foreach (var comment in userData.MyComments)
                    {
                        MyComments.Add(comment);
                    }
                }

                // Notify that RoundedRating may have changed
                OnPropertyChanged(nameof(RoundedRating));
            }
            else
            {
                ErrorMessage = $"Failed to load profile: {message}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading profile: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ShowEventCommentsTab()
    {
        ShowEventComments = true;
        ShowMyComments = false;
    }

    [RelayCommand]
    private void ShowMyCommentsTab()
    {
        ShowEventComments = false;
        ShowMyComments = true;
    }

    [RelayCommand]
    private async Task NavigateToMyTickets()
    {
        await MainPageView.Instance.mainPageViewModel.NavigateToMyTicketsCommand.ExecuteAsync(null);
    }

    public void ClearFormData()
    {
        UserName = string.Empty;
        Email = string.Empty;
        Organization = string.Empty;
        Rating = 0.0;
        IsOrganizer = false;
        EventComments.Clear();
        MyComments.Clear();
        ErrorMessage = string.Empty;
        ShowEventComments = true;
        ShowMyComments = false;
        OnPropertyChanged(nameof(RoundedRating));
    }
}