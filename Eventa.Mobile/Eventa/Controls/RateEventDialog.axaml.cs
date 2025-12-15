using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Eventa.Models.Comments;
using Eventa.Models.Ordering;
using Eventa.Services;
using Eventa.Views.Main;
using System;
using System.Threading.Tasks;

namespace Eventa.Controls;

public partial class RateEventDialog : UserControl
{
    private static RateEventDialog? _instance;
    private readonly ApiService _apiService = new();
    private int _selectedRating = 0;
    private OrderListItemResponseModel? _currentOrder;
    private string? _jwtToken;
    private TaskCompletionSource<CommentDataModel?>? _taskCompletionSource;

    public static RateEventDialog Instance
    {
        get
        {
            _instance ??= new RateEventDialog();
            return _instance;
        }
    }

    public RateEventDialog()
    {
        InitializeComponent();
    }

    public Task<CommentDataModel?> Show(OrderListItemResponseModel order, string jwtToken)
    {
        MainView.Instance.mainViewModel.ShowRatingDialog();
        _taskCompletionSource = new TaskCompletionSource<CommentDataModel?>();
        _currentOrder = order;
        _jwtToken = jwtToken;
        _selectedRating = 0;

        // Set event details
        EventNameTextBlock.Text = order.EventName;
        EventDateTextBlock.Text = order.EventDateTime.ToString("dd MMM yyyy, HH:mm");
        EventPlaceTextBlock.Text = ""; // You can add place info if available

        // Reset stars
        UpdateStarColors();

        // Clear comment and error
        CommentTextBox.Text = string.Empty;
        ErrorTextBlock.IsVisible = false;
        ErrorTextBlock.Text = string.Empty;

        // Show the dialog
        DialogOverlay.IsVisible = true;

        return _taskCompletionSource.Task;
    }

    private void OnStar1Click(object? sender, RoutedEventArgs e) => SetRating(1);
    private void OnStar2Click(object? sender, RoutedEventArgs e) => SetRating(2);
    private void OnStar3Click(object? sender, RoutedEventArgs e) => SetRating(3);
    private void OnStar4Click(object? sender, RoutedEventArgs e) => SetRating(4);
    private void OnStar5Click(object? sender, RoutedEventArgs e) => SetRating(5);

    private void SetRating(int rating)
    {
        _selectedRating = rating;
        UpdateStarColors();
    }

    private void UpdateStarColors()
    {
        var filledColor = new SolidColorBrush(Color.Parse("#FFB5B5"));
        var emptyColor = new SolidColorBrush(Color.Parse("#E0E0E0"));

        Star1Icon.Foreground = _selectedRating >= 1 ? filledColor : emptyColor;
        Star2Icon.Foreground = _selectedRating >= 2 ? filledColor : emptyColor;
        Star3Icon.Foreground = _selectedRating >= 3 ? filledColor : emptyColor;
        Star4Icon.Foreground = _selectedRating >= 4 ? filledColor : emptyColor;
        Star5Icon.Foreground = _selectedRating >= 5 ? filledColor : emptyColor;
    }

    private async void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        if (_selectedRating == 0)
        {
            ErrorTextBlock.Text = "Please select a rating";
            ErrorTextBlock.IsVisible = true;
            return;
        }

        if (_currentOrder == null || string.IsNullOrEmpty(_jwtToken))
        {
            ErrorTextBlock.Text = "Error: Missing order information";
            ErrorTextBlock.IsVisible = true;
            return;
        }

        try
        {
            ConfirmButton.IsEnabled = false;
            ErrorTextBlock.IsVisible = false;

            var (success, message, data) = await _apiService.CreateCommentAsync(
                _currentOrder.OrderId,
                _selectedRating,
                CommentTextBox.Text,
                _jwtToken
            );

            if (success && data != null)
            {
                DialogOverlay.IsVisible = false;
                _taskCompletionSource?.SetResult(data);
                MainPageView.Instance.mainPageViewModel.NavigateToMyTicketsCommand.Execute(null);
            }
            else
            {
                ErrorTextBlock.Text = message;
                ErrorTextBlock.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorTextBlock.Text = $"Error: {ex.Message}";
            ErrorTextBlock.IsVisible = true;
        }
        finally
        {
            ConfirmButton.IsEnabled = true;
        }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        DialogOverlay.IsVisible = false;
        _taskCompletionSource?.SetResult(null);
        MainPageView.Instance.mainPageViewModel.NavigateToMyTicketsCommand.Execute(null);
    }
}