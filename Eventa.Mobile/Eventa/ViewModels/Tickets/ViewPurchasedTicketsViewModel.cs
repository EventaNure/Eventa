using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Models.Ordering;
using Eventa.Services;
using Eventa.Views.Main;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Tickets;

public partial class ViewPurchasedTicketsViewModel : ObservableObject
{
    private readonly ApiService _apiService = new();

    [ObservableProperty]
    private ObservableCollection<OrderListItemResponseModel> _orders = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _noTickets;

    public async Task LoadTicketsAsync()
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = string.Empty;
        NoTickets = false;

        try
        {
            var jwtToken = MainPageView.Instance.mainPageViewModel.JwtToken;

            if (string.IsNullOrEmpty(jwtToken))
            {
                HasError = true;
                ErrorMessage = "You must be logged in to view your tickets.";
                IsLoading = false;
                return;
            }

            var (success, message, data) = await _apiService.GetOrdersByUserAsync(jwtToken);

            if (success && data != null)
            {
                Orders = new ObservableCollection<OrderListItemResponseModel>(data);
                NoTickets = !Orders.Any();
            }
            else
            {
                HasError = true;
                ErrorMessage = message;
            }
        }
        catch (System.Exception ex)
        {
            HasError = true;
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ViewQRCode(OrderListItemResponseModel order)
    {
    }

    public void ClearFormData()
    {
        Orders.Clear();
        NoTickets = false;
        HasError = false;
        ErrorMessage = string.Empty;
    }
}