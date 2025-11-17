using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventa.Controls;
using Eventa.Models.Payment;
using Eventa.Services;
using Eventa.Views.Events;
using Eventa.Views.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Eventa.ViewModels.Ordering;

public partial class TicketOrderViewModel : ObservableObject
{
    private readonly ApiService _apiService = new();
    private string _jwtToken = string.Empty;
    private CancellationTokenSource? _timerCancellation;
    private readonly StripePaymentService _stripeService;

    [ObservableProperty]
    private string _eventName = string.Empty;

    [ObservableProperty]
    private string _eventDateTime = string.Empty;

    [ObservableProperty]
    private double _totalPrice = 0.0;

    [ObservableProperty]
    private ObservableCollection<OrderTicketViewModel> _ticketsInOrder = [];

    [ObservableProperty]
    private string _cardNumber = string.Empty;

    [ObservableProperty]
    private string _expiryDate = string.Empty;

    [ObservableProperty]
    private string _cvc = string.Empty;

    [ObservableProperty]
    private bool _isProcessing = false;

    [ObservableProperty]
    private string _submitButtonText = "Submit";

    [ObservableProperty]
    private string _timeLeftDisplay = "Loading...";

    [ObservableProperty]
    private bool _hasTickets = false;

    private string _sessionId = string.Empty;

    public TicketOrderViewModel()
    {
        _stripeService = new StripePaymentService(
              "",
              ""
        );
    }

    public async Task LoadOrderDataAsync(string jwtToken, string eventDateTime, TimeSpan expireDate, string sessionId)
    {
        _jwtToken = jwtToken;
        EventDateTime = eventDateTime;
        _sessionId = sessionId;
        try
        {
            var (success, message, ordersData) = await _apiService.GetTicketsInCartByUserAsync(_jwtToken);

            if (!success || ordersData == null || ordersData == null)
            {
                await DialogControl.Instance.Show("Error", $"Failed to load order: {message}", "OK");
                ClearFormData();
                return;
            }

            var latestOrder = ordersData;

            TicketsInOrder.Clear();

            if (latestOrder.Tickets != null && latestOrder.Tickets.Count > 0)
            {
                EventName = latestOrder.EventName;
                TotalPrice = latestOrder.TotalCost;

                foreach (var ticket in latestOrder.Tickets)
                {
                    var orderTicket = new OrderTicketViewModel
                    {
                        SeatId = ticket.SeatId,
                        RowTypeName = ticket.RowTypeName,
                        RowNumber = ticket.Row,
                        SeatNumber = ticket.SeatNumber,
                        Price = ticket.Price
                    };
                    TicketsInOrder.Add(orderTicket);
                }

                HasTickets = true;

                StartCountdownTimer(expireDate);
            }
            else
            {
                ClearFormData();
            }
        }
        catch (Exception ex)
        {
            await DialogControl.Instance.Show("Error", $"An error occurred: {ex.Message}", "OK");
            ClearFormData();
        }
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

                    // Wait for 1 second
                    await Task.Delay(1000, _timerCancellation.Token);

                    // Decrease time by 1 second
                    timeRemaining = timeRemaining.Subtract(TimeSpan.FromSeconds(1));
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    // Continue even if one iteration fails
                    await Task.Delay(1000, _timerCancellation.Token);
                }
            }

            // Time expired - clear order
            if (timeRemaining.TotalSeconds <= 0)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await DialogControl.Instance.Show("Time Expired", "Your booking time has expired. The tickets have been released.", "OK");
                    MainPageView.Instance.mainPageViewModel.CurrentPage = BrowseEventsView.Instance;
                    ClearFormData();
                });
            }
        }, _timerCancellation.Token);
    }

    public void ClearFormData()
    {
        _timerCancellation?.Cancel();
        TicketsInOrder.Clear();
        TotalPrice = 0.0;
        HasTickets = false;
        TimeLeftDisplay = "No active booking";
        CardNumber = string.Empty;
        ExpiryDate = string.Empty;
        Cvc = string.Empty;
    }

    [RelayCommand]
    private async Task SubmitPayment()
    {
        if (!ValidateCardInfo())
            return;

        IsProcessing = true;
        SubmitButtonText = "Processing...";

        try
        {
            _timerCancellation?.Cancel();

            // Parse expiry date
            var expiryParts = ExpiryDate.Split('/');
            if (expiryParts.Length != 2 ||
                !long.TryParse(expiryParts[0], out var month) ||
                !long.TryParse(expiryParts[1], out var year))
            {
                await DialogControl.Instance.Show("Invalid Date", "Please enter a valid expiry date (MM/YY)", "OK");
                return;
            }

            // Convert 2-digit year to 4-digit
            year += year < 100 ? 2000 : 0;

            // Create payment request
            var paymentRequest = new PaymentRequest
            {
                CardNumber = CardNumber,
                ExpiryMonth = month,
                ExpiryYear = year,
                Cvc = Cvc,
                Amount = (decimal)TotalPrice,
                Currency = "uah",
                Description = $"Ticket purchase for {EventName}",
                Metadata = new Dictionary<string, string>
            {
                { "event_name", EventName },
                { "ticket_count", TicketsInOrder.Count.ToString() },
                { "event_datetime", EventDateTime.ToString() }
            }
            };

            var result = await _stripeService.ProcessCardPayment(paymentRequest, _sessionId);

            if (result.Success)
            {
                SubmitButtonText = "Payment Successful!";
                await Task.Delay(1500);
                await DialogControl.Instance.Show(
                    "Success",
                    $"Payment successful! Transaction ID: {result.TransactionId}\nYour tickets have been confirmed.",
                    "OK"
                );
                MainPageView.Instance.mainPageViewModel.HeaderTitleClickedCommand.Execute(null);
            }
            else
            {
                await DialogControl.Instance.Show("Payment Failed", result.Message!, "OK");
            }
        }
        catch (Exception ex)
        {
            await DialogControl.Instance.Show("Payment Failed", $"Payment failed: {ex.Message}", "OK");
        }
        finally
        {
            IsProcessing = false;
            SubmitButtonText = "Submit";
        }
    }

    private bool ValidateCardInfo()
    {
        var cardDigits = Regex.Replace(CardNumber, @"[^\d]", "");

        if (string.IsNullOrWhiteSpace(cardDigits) || cardDigits.Length < 13 || cardDigits.Length > 16)
        {
            DialogControl.Instance.Show("Invalid Card", "Please enter a valid card number", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ExpiryDate) || !Regex.IsMatch(ExpiryDate, @"^\d{2}/\d{2}$"))
        {
            DialogControl.Instance.Show("Invalid Expiry", "Please enter a valid expiry date (MM/YY)", "OK");
            return false;
        }

        var parts = ExpiryDate.Split('/');
        var month = int.Parse(parts[0]);
        var year = int.Parse("20" + parts[1]);

        if (month < 1 || month > 12)
        {
            DialogControl.Instance.Show("Invalid Month", "Invalid expiry month", "OK");
            return false;
        }

        var expiryDate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
        if (expiryDate < DateTime.Now)
        {
            DialogControl.Instance.Show("Card Expired", "Card has expired", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Cvc) || Cvc.Length < 3)
        {
            DialogControl.Instance.Show("Invalid CVC", "Please enter a valid CVC", "OK");
            return false;
        }

        if (TicketsInOrder.Count == 0)
        {
            DialogControl.Instance.Show("No Tickets", "No tickets in order", "OK");
            return false;
        }

        return true;
    }
}