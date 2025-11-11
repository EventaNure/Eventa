using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Config;
using Eventa.Controls;
using Eventa.Models.Booking;
using Eventa.Models.Carting;
using Eventa.Models.Events.Organizer;
using Eventa.Models.Seats;
using Eventa.Services;
using Eventa.Views.Events;
using Eventa.Views.Main;
using Eventa.Views.Ordering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Ordering;

public partial class SeatOrderViewModel : ObservableObject
{
    private readonly JsonSettings<AppSettings> _settingsService = new();

    private static readonly List<Color> RowTypeColors =
    [
        Color.Parse("#FFB5B5"), // Pink
        Color.Parse("#FFD9A0"), // Orange
        Color.Parse("#B5C9FF"), // Blue
        Color.Parse("#B5FFB5"), // Green
        Color.Parse("#E0B5FF"), // Purple
        Color.Parse("#FFE5B5"), // Yellow
    ];

    private readonly ApiService _apiService = new();
    private string _jwtToken = string.Empty;
    private CancellationTokenSource? _timerCancellation;

    [ObservableProperty]
    private string _hallPlanUrl = string.Empty;

    [ObservableProperty]
    private string _eventName = string.Empty;

    [ObservableProperty]
    private string _eventDateTime = string.Empty;

    [ObservableProperty]
    private double _totalPrice = 0.0;

    [ObservableProperty]
    private ObservableCollection<RowTypeViewModel> _rowTypes = [];

    [ObservableProperty]
    private ObservableCollection<SeatViewModel> _selectedSeats = [];

    [ObservableProperty]
    private bool _isBooking = false;

    [ObservableProperty]
    private string _timeLeftDisplay = "Loading...";

    private int _eventDateTimeId;

    public bool HasSelectedSeats => SelectedSeats.Count > 0;

    public SeatOrderViewModel()
    {
        SelectedSeats.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasSelectedSeats));
    }

    public async Task InsertFormData(
     FreeSeatsWithHallPlanResponseModel data,
     string eventName,
     EventDateTimes eventDateTimes,
     string jwtToken)
    {
        _eventDateTimeId = eventDateTimes.Id;
        _jwtToken = jwtToken;

        HallPlanUrl = data.HallPlanUrl;
        EventName = eventName;
        EventDateTime = eventDateTimes.DateTime.ToString("dd MMM yyyy, HH:mm");

        var settings = await _settingsService.LoadAsync();
        settings.CartDateTime = EventDateTime;
        await _settingsService.SaveAsync(settings);

        SelectedSeats.Clear();
        TotalPrice = 0.0;

        RebuildRowTypes(data);

        var (cartSuccess, cartMessage, cartData) = await _apiService.GetTicketsInCartByUserAsync(jwtToken);

        if (cartSuccess && cartData != null && cartData.Tickets.Count > 0)
        {
            await LoadCartOnFormLoadAsync(cartData, jwtToken);

            var (timeSuccess, _, timeLeft) = await _apiService.GetBookingTimeLeftAsync(jwtToken);
            if (timeSuccess && timeLeft.HasValue && timeLeft.Value.TotalSeconds > 0)
            {
                StartCountdownTimer(timeLeft.Value);
            }
            else
            {
                TimeLeftDisplay = "No active booking";
            }
        }
        else
        {
            TimeLeftDisplay = "No active booking";
        }
    }

    private void RebuildRowTypes(FreeSeatsWithHallPlanResponseModel data)
    {
        RowTypes.Clear();
        int colorIndex = 0;

        foreach (var rowType in data.RowTypes)
        {
            var color = RowTypeColors[colorIndex % RowTypeColors.Count];
            colorIndex++;

            var rowTypeVm = new RowTypeViewModel
            {
                Name = rowType.Name,
                Color = color,
                Rows = []
            };

            var allSeats = rowType.Rows.SelectMany(r => r.Seats).ToList();
            if (allSeats.Count != 0)
            {
                var minPrice = allSeats.Min(s => s.Price);
                var maxPrice = allSeats.Max(s => s.Price);
                rowTypeVm.PriceRange = minPrice == maxPrice
                    ? $"[{minPrice:F2}]"
                    : $"[{minPrice:F2}-{maxPrice:F2}]";
            }

            foreach (var row in rowType.Rows)
            {
                var rowVm = new RowViewModel
                {
                    Id = row.Id,
                    RowNumber = row.RowNumber,
                    Seats = []
                };

                foreach (var seat in row.Seats)
                {
                    var seatVm = new SeatViewModel(color)
                    {
                        Id = seat.Id,
                        SeatNumber = seat.SeatNumber,
                        Price = seat.Price,
                        RowTypeName = rowType.Name,
                        RowNumber = row.RowNumber,
                        SelectSeatCommand = new RelayCommand<SeatViewModel>(ToggleSeatSelection)
                    };
                    rowVm.Seats.Add(seatVm);
                }

                rowTypeVm.Rows.Add(rowVm);
            }

            RowTypes.Add(rowTypeVm);
        }
    }

    public Task LoadCartOnFormLoadAsync(CartResponseModel cartData, string jwtToken)
    {
        _jwtToken = jwtToken;

        if (cartData?.Tickets == null || cartData.Tickets.Count == 0)
            return Task.CompletedTask;

        SelectedSeats.Clear();

        foreach (var ticket in cartData.Tickets)
        {
            var seatVm = RowTypes
                .SelectMany(rt => rt.Rows)
                .SelectMany(r => r.Seats)
                .FirstOrDefault(s => s.Id == ticket.SeatId);

            if (seatVm != null)
            {
                seatVm.IsSelected = true;
                SelectedSeats.Add(seatVm);
            }
            else
            {
                var missingSeat = new SeatViewModel(Colors.Gray)
                {
                    Id = ticket.SeatId,
                    RowTypeName = ticket.RowTypeName ?? "Reserved",
                    RowNumber = ticket.Row,
                    SeatNumber = ticket.SeatNumber,
                    Price = ticket.Price,
                    IsSelected = true
                };

                SelectedSeats.Add(missingSeat);
            }
        }

        TotalPrice = cartData.TotalCost;
        return Task.CompletedTask;
    }

    private void StartCountdownTimer(TimeSpan initialTimeLeft)
    {
        _timerCancellation?.Cancel();
        _timerCancellation = new CancellationTokenSource();

        Task.Run(async () =>
        {
            var timeRemaining = initialTimeLeft;

            while (!_timerCancellation.Token.IsCancellationRequested && timeRemaining.TotalSeconds > 0)
            {
                try
                {
                    // Update timer display
                    var minutes = (int)timeRemaining.TotalMinutes;
                    var seconds = timeRemaining.Seconds;

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        TimeLeftDisplay = $"Time left: {minutes:D2}:{seconds:D2}";
                    });

                    await Task.Delay(1000, _timerCancellation.Token);

                    timeRemaining = timeRemaining.Subtract(TimeSpan.FromSeconds(1));
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    await Task.Delay(1000, _timerCancellation.Token);
                }
            }

            if (timeRemaining.TotalSeconds <= 0 && !_timerCancellation.Token.IsCancellationRequested)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await DialogControl.Instance.Show("Time Expired", "Your booking time has expired. The tickets have been released.", "OK");
                    MainPageView.Instance.mainPageViewModel.HeaderTitleClickedCommand.Execute(null);
                    ClearFormData();
                });
            }
        }, _timerCancellation.Token);
    }

    private async void ToggleSeatSelection(SeatViewModel? seat)
    {
        if (seat == null) return;

        if (string.IsNullOrEmpty(_jwtToken))
        {
            bool result = await DialogControl.Instance.Show("You are not logged in!", "You must be logged in to select seats.", "Remain", "Login");
            if (result)
            {
                MainPageView.Instance.mainPageViewModel.LoginCommand.Execute(null);
            }
            return;
        }

        if (seat.IsSelected)
        {
            var (success, message, cartData) = await _apiService.DeleteTicketFromCartAsync(seat.Id, _jwtToken);

            if (success)
            {
                seat.IsSelected = false;
                SelectedSeats.Remove(seat);

                if (cartData != null)
                {
                    TotalPrice = cartData.TotalCost;
                }
                else
                {
                    CalculateTotalPrice();
                }

                if (SelectedSeats.Count == 0)
                {
                    _timerCancellation?.Cancel();
                    TimeLeftDisplay = "No active booking";
                }
            }
            else
            {
                await DialogControl.Instance.Show("Error", $"Failed to remove seat: {message}", "OK");
            }
        }
        else
        {
            var bookingRequest = new BookTicketRequestModel
            {
                EventDateTimeId = _eventDateTimeId,
                SeatId = seat.Id
            };

            var (success, message, cartData) = await _apiService.AddTicketToCartAsync(bookingRequest, _jwtToken);

            if (success)
            {
                seat.IsSelected = true;
                SelectedSeats.Add(seat);

                if (cartData != null)
                {
                    TotalPrice = cartData.TotalCost;
                    var (timeSuccess, _, timeLeft) = await _apiService.GetBookingTimeLeftAsync(_jwtToken);
                    if (timeSuccess && timeLeft.HasValue && timeLeft.Value.TotalSeconds > 0)
                    {
                        StartCountdownTimer(timeLeft.Value);
                    }
                }
                else
                {
                    CalculateTotalPrice();
                }
            }
            else
            {
                await DialogControl.Instance.Show("Booking Failed", $"Failed to add seat: {message}", "OK");
            }
        }
    }

    private void CalculateTotalPrice()
    {
        TotalPrice = SelectedSeats.Sum(s => s.Price);
    }

    [RelayCommand]
    private async Task BuyAsync()
    {
        if (SelectedSeats.Count == 0)
        {
            await DialogControl.Instance.Show("No seats selected", "Please select at least one seat to continue.", "OK");
            return;
        }

        if (string.IsNullOrEmpty(_jwtToken))
        {
            bool result = await DialogControl.Instance.Show("You are not logged in!", "You must be logged in to proceed with the purchase.", "Remain", "Login");
            if (result)
            {
                MainPageView.Instance.mainPageViewModel.LoginCommand.Execute(null);
            }
            return;
        }

        IsBooking = true;

        try
        {
            var (success, message, orderData) = await _apiService.CreateOrderAsync(_jwtToken);

            if (!success || orderData == null)
            {
                await DialogControl.Instance.Show("Order Failed", $"Failed to create order: {message}", "OK");
                IsBooking = false;
                return;
            }

            _timerCancellation?.Cancel();

            var ticketOrderView = TicketOrderView.Instance;

            await ticketOrderView.ticketOrderViewModel.LoadOrderDataAsync(_jwtToken, EventDateTime, orderData.ExpireAt);

            MainPageView.Instance.mainPageViewModel.CurrentPage = ticketOrderView;
        }
        catch (Exception ex)
        {
            await DialogControl.Instance.Show("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            IsBooking = false;
        }
    }

    [RelayCommand]
    private async Task RemoveSeat(SeatViewModel? seat)
    {
        if (seat is null || string.IsNullOrEmpty(_jwtToken))
            return;

        var (success, message, cartData) = await _apiService.DeleteTicketFromCartAsync(seat.Id, _jwtToken);

        if (success)
        {
            SelectedSeats.Remove(seat);

            if (cartData != null)
            {
                TotalPrice = cartData.TotalCost;
            }
            else
            {
                CalculateTotalPrice();
            }

            if (SelectedSeats.Count == 0)
            {
                _timerCancellation?.Cancel();
                TimeLeftDisplay = "No active booking";
            }

            var seatInHallPlan = RowTypes
                .SelectMany(rt => rt.Rows)
                .SelectMany(r => r.Seats)
                .FirstOrDefault(s => s.Id == seat.Id);

            if (seatInHallPlan != null)
            {
                seatInHallPlan.IsSelected = false;
            }
            else
            {
                var targetRowType = RowTypes.FirstOrDefault(rt => rt.Name == seat.RowTypeName);
                if (targetRowType != null)
                {
                    var targetRow = targetRowType.Rows.FirstOrDefault(r => r.RowNumber == seat.RowNumber);
                    if (targetRow != null)
                    {
                        var newRealSeat = new SeatViewModel(targetRowType.Color)
                        {
                            Id = seat.Id,
                            SeatNumber = seat.SeatNumber,
                            Price = seat.Price,
                            RowTypeName = seat.RowTypeName,
                            RowNumber = seat.RowNumber,
                            IsSelected = false,
                            SelectSeatCommand = new RelayCommand<SeatViewModel>(ToggleSeatSelection)
                        };

                        targetRow.Seats.Add(newRealSeat);

                        var sortedSeats = targetRow.Seats.OrderBy(s => s.SeatNumber).ToList();
                        targetRow.Seats.Clear();
                        foreach (var s in sortedSeats)
                        {
                            targetRow.Seats.Add(s);
                        }
                    }
                }
            }
        }
        else
        {
            await DialogControl.Instance.Show("Error", $"Failed to remove seat: {message}", "OK");
        }
    }

    public void ClearFormData()
    {
        _timerCancellation?.Cancel();
        SelectedSeats.Clear();
        RowTypes.Clear();
        TotalPrice = 0.0;
        TimeLeftDisplay = "No active booking";
        EventDateTime = string.Empty;
        IsBooking = false;
    }
}