using Eventa.Converters;
using Eventa.Models.Authentication;
using Eventa.Models.Booking;
using Eventa.Models.Carting;
using Eventa.Models.Events;
using Eventa.Models.Events.Organizer;
using Eventa.Models.Ordering;
using Eventa.Models.Seats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eventa.Services;

public class ApiService
{
    private static readonly HttpClient _httpClient;

    // LOCAL DEV:
    private const string BaseUrlDesktop = "https://localhost:7293";
    private const string BaseUrlAndroid = "https://10.0.2.2:7293";
    //PROD:
    // private const string BaseUrlDesktop = "https://eventa-app.fun:5001";
    // private const string BaseUrlAndroid = "https://eventa-app.fun:5001";

    static ApiService()
    {
        if (OperatingSystem.IsBrowser())
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(BaseUrlDesktop),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }
        else
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
        }

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
                { new StringContent(model.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price" },
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
                var extension = Path.GetExtension(model.ImageFileName).ToLowerInvariant();
                var mimeType = extension switch
                {
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };

                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
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
                { new StringContent(model.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price" },
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

                var extension = Path.GetExtension(model.ImageFileName).ToLowerInvariant();
                var mimeType = extension switch
                {
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };

                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
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

    public async Task<byte[]?> DownloadImageAsync(string imageUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync(imageUrl);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<(bool Success, string Message, string? ImageUrl)> UploadTempImageAsync(byte[] imageFile, string fileName, string jwtToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            using var content = new MultipartFormDataContent();

            var imageContent = new ByteArrayContent(imageFile);
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var mimeType = extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
            content.Add(imageContent, "ImageFile", fileName);

            var response = await _httpClient.PostAsync("/api/Events/temp-image", content);

            if (response.IsSuccessStatusCode)
            {
                var imageUrl = await response.Content.ReadAsStringAsync();
                return (true, "Image uploaded successfully!", imageUrl.Trim('"'));
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

    // Orders methods
    public async Task<(bool Success, string Message, OrderResponseModel? Data)> CreateOrderAsync(string jwtToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.PostAsync("/api/Orders", null);

            if (response.IsSuccessStatusCode)
            {
                var order = await response.Content.ReadFromJsonAsync<OrderResponseModel>();
                return (true, "Order created successfully!", order);
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

    public async Task<(bool Success, string Message, List<OrderListItemResponseModel>? Data)> GetOrdersByUserAsync(string jwtToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.GetAsync("/api/Orders/by-user");

            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadFromJsonAsync<List<OrderListItemResponseModel>>();
                return (true, "Orders fetched successfully!", orders);
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

    // Seats methods
    public async Task<(bool Success, string Message, FreeSeatsWithHallPlanResponseModel? Data)> GetFreeSeatsWithHallPlanAsync(int eventDateTimeId, string? jwtToken = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(jwtToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            var response = await _httpClient.GetAsync($"/api/Seats/free-with-hall-plan?eventDateTimeId={eventDateTimeId}");

            if (response.IsSuccessStatusCode)
            {
                var seats = await response.Content.ReadFromJsonAsync<FreeSeatsWithHallPlanResponseModel>();
                return (true, "Seats fetched successfully!", seats);
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

    // Tickets in Cart methods
    public async Task<(bool Success, string Message, CartResponseModel? Data)> AddTicketToCartAsync(BookTicketRequestModel model, string jwtToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.PostAsJsonAsync("/api/TicketsInCart", model);

            if (response.IsSuccessStatusCode)
            {
                var cart = await response.Content.ReadFromJsonAsync<CartResponseModel>();
                return (true, "Ticket added to cart successfully!", cart);
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

    public async Task<(bool Success, string Message, CartResponseModel? Data)> GetTicketsInCartByUserAsync(string jwtToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.GetAsync("/api/TicketsInCart/by-user");

            if (response.IsSuccessStatusCode)
            {
                var cart = await response.Content.ReadFromJsonAsync<CartResponseModel>();
                return (true, "Cart fetched successfully!", cart);
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

    public async Task<(bool Success, string Message, CartResponseModel? Data)> DeleteTicketFromCartAsync(int ticketId, string jwtToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.DeleteAsync($"/api/TicketsInCart/{ticketId}");

            if (response.IsSuccessStatusCode)
            {
                var cart = await response.Content.ReadFromJsonAsync<CartResponseModel>();
                return (true, "Ticket removed from cart successfully!", cart);
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

    public async Task<(bool Success, string Message, TimeSpan? Data)> GetBookingTimeLeftAsync(string jwtToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.GetAsync("/api/User/tickets-in-cart/time-left");

            if (response.IsSuccessStatusCode)
            {
                var timeLeft = await response.Content.ReadFromJsonAsync<TimeSpan>();
                return (true, "Time left fetched successfully!", timeLeft);
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
}