using Eventa.Converters;
using Eventa.Models.Authentication;
using Eventa.Models.Events;
using Eventa.Models.Events.Organizer;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eventa.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrlDesktop = "https://localhost:7293";
    private const string BaseUrlAndroid = "https://10.0.2.2:7293";

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

    // Authentication methods
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

    public async Task<(bool Success, string Message, object? Data)> GetMainTagsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/Tags/main");

            if (response.IsSuccessStatusCode)
            {
                var tags = await response.Content.ReadFromJsonAsync<JsonArray>();
                return (true, "Tags fetched successfully!", tags);
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

    public async Task<(bool Success, string Message, List<TagResponseModel>? Data)> GetAllTagsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/Tags");

            if (response.IsSuccessStatusCode)
            {
                var tags = await response.Content.ReadFromJsonAsync<List<TagResponseModel>>();
                return (true, "Tags fetched successfully!", tags);
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

    public async Task<(bool Success, string Message, List<EventResponseModel>? Data)> GetEventsAsync(EventsRequestModel request)
    {
        try
        {
            var queryString = request.ToQueryString();
            var response = await _httpClient.GetAsync($"/api/Events?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var events = await response.Content.ReadFromJsonAsync<List<EventResponseModel>>();
                return (true, "Events fetched successfully!", events);
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

    public async Task<(bool Success, string Message, EventDetailsResponseModel? Data)> GetEventByIdAsync(int eventId, string jwtToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(jwtToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            var response = await _httpClient.GetAsync($"/api/Events/{eventId}");

            if (response.IsSuccessStatusCode)
            {
                var eventDetails = await response.Content.ReadFromJsonAsync<EventDetailsResponseModel>();
                return (true, "Event details fetched successfully!", eventDetails);
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
        finally
        {
            if (!string.IsNullOrEmpty(jwtToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }

    public async Task<(bool Success, string Message, List<OrganizerEventResponseModel>? Data)> GetOrganizerEventsAsync(string jwtToken, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.GetAsync($"/api/Events/by-organizer?pageNumber={pageNumber}&pageSize={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                var events = await response.Content.ReadFromJsonAsync<List<OrganizerEventResponseModel>>();
                return (true, "Organizer events fetched successfully!", events);
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
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<(bool Success, string Message)> CreateEventAsync(CreateEventRequestModel model, string jwtToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            using var content = new MultipartFormDataContent
            {
                { new StringContent(model.Title), "Title" },
                { new StringContent(model.Description), "Description" },
                { new StringContent(model.Price.ToString()), "Price" },
                { new StringContent(model.Duration.ToString()), "Duration" },
                { new StringContent(model.OrganizerId), "OrganizerId" },
                { new StringContent(model.PlaceId.ToString()), "PlaceId" }
            };

            foreach (var tagId in model.TagIds)
            {
                content.Add(new StringContent(tagId.ToString()), "TagIds");
            }

            foreach (var dateTime in model.DateTimes)
            {
                content.Add(new StringContent(dateTime.ToString("o")), "DateTimes");
            }

            if (model.ImageFile != null && model.ImageFileName != null)
            {
                var imageContent = new ByteArrayContent(model.ImageFile);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpg");
                content.Add(imageContent, "ImageFile", model.ImageFileName);
            }

            var response = await _httpClient.PostAsync("/api/Events", content);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Event created successfully!");
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
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }


    public async Task<(bool Success, string Message)> UpdateEventAsync(int eventId, UpdateEventRequestModel model, string jwtToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            using var content = new MultipartFormDataContent
            {
                { new StringContent(model.Title), "Title" },
                { new StringContent(model.Description), "Description" },
                { new StringContent(model.Price.ToString()), "Price" },
                { new StringContent(model.Duration.ToString()), "Duration" },
                { new StringContent(model.OrganizerId), "OrganizerId" },
                { new StringContent(model.PlaceId.ToString()), "PlaceId" }
            };

            foreach (var tagId in model.TagIds)
            {
                content.Add(new StringContent(tagId.ToString()), "TagIds");
            }

            foreach (var dateTime in model.DateTimes)
            {
                content.Add(new StringContent(dateTime.ToString("o")), "DateTimes");
            }

            if (model.ImageFile != null && model.ImageFileName != null)
            {
                var imageContent = new ByteArrayContent(model.ImageFile);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpg");
                content.Add(imageContent, "ImageFile", model.ImageFileName);
            }

            var response = await _httpClient.PutAsync($"/api/Events/{eventId}", content);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Event updated successfully!");
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
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<(bool Success, string Message)> DeleteEventAsync(int eventId, string jwtToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.DeleteAsync($"/api/Events/{eventId}");

            if (response.IsSuccessStatusCode)
            {
                return (true, "Event deleted successfully!");
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
        finally
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<(bool Success, string Message, List<PlaceResponseModel>? Data)> GetPlacesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/Places");

            if (response.IsSuccessStatusCode)
            {
                var places = await response.Content.ReadFromJsonAsync<List<PlaceResponseModel>>();
                return (true, "Places fetched successfully!", places);
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