using Eventa.Converters;
using Eventa.Models;
using Eventa.Models.Authentication;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eventa.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrlDesktop = "https://localhost:7293"; // For Desktop
    private const string BaseUrlAndroid = "https://10.0.2.2:7293"; // For Android Emulator
    public ApiService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(OperatingSystem.IsAndroid() ? BaseUrlAndroid : BaseUrlDesktop),
            Timeout = TimeSpan.FromSeconds(30)
        };

        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<(bool Success, string Message, object? Data)> RegisterAsync(RegisterRequestModel model)
    {
        try
        {
            string query = string.IsNullOrEmpty(model.OrganizationName)
                ? "/api/User/register-user"
                : "/api/User/register-organizer";

            var response = await _httpClient.PostAsJsonAsync(query, model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return (true, "Registration successful. Please check your email for verification code.", result);
            }

            var errorMessage = await ApiErrorConverter.ExtractErrorMessageAsync(response);
            return (false, errorMessage, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Network error: {ex.Message}", null);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message)> ConfirmEmailAsync(EmailConfirmationRequestModel model)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/User/confirm-email", model);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Email confirmed successfully!");
            }

            var errorMessage = await ApiErrorConverter.ExtractErrorMessageAsync(response);
            return (false, errorMessage);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> ResendConfirmEmailAsync(ResendEmailConfirmationRequestModel model)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/User/resend-confirm-email", model);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Confirmation email resent successfully!");
            }

            var errorMessage = await ApiErrorConverter.ExtractErrorMessageAsync(response);
            return (false, errorMessage);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message, object? Data)> LoginAsync(LoginRequestModel model)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/User/login", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return (true, "Login successful!", result);
            }

            var errorMessage = await ApiErrorConverter.ExtractErrorMessageAsync(response);
            return (false, errorMessage, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Network error: {ex.Message}", null);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}", null);
        }
    }
}