using Eventa.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eventa.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrlDesktop = "https://localhost:7293"; // For Desktop
        private const string BaseUrlAndroid = "https://10.0.2.2:7293"; // For Android Emulator
        public ApiService()
        {
            var handler = new HttpClientHandler
            {
                // Accept self-signed certificates for local development
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
                var response = await _httpClient.PostAsJsonAsync("/api/User/register", model);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    return (true, "Registration successful. Please check your email for verification code.", result);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, $"Registration failed: {errorContent}", null);
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

                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, $"Confirmation failed: {errorContent}");
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
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
                    return (true, "Login successful!", result?.Data);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, $"Login failed: {errorContent}", null);
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
}