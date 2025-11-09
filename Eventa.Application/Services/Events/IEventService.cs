using Eventa.Application.DTOs.Events;
using FluentResults;

namespace Eventa.Application.Services.Events
{
    public interface IEventService
    {
        Task<Result<int>> CreateEventAsync(CreateEventDto dto);
        Task<Result> DeleteEventAsync(int eventId, string organizerId);
        Task<Result<EventDto>> GetEventAsync(int eventId);
        Task<List<EventListItemDto>> GetEventsAsync(int pageNumber, int pageSize, IEnumerable<int> tagIds, DateOnly? startDate, DateOnly? endDate, string? subName);
        Task<List<EventListItemDto>> GetEventsByOrganizerAsync(int pageNumber, int pageSize, string organizerId);
        Task<Result<string>> LoadImageAsync(Stream stream, string fileName);
        Task<Result> UpdateEventAsync(UpdateEventDto dto);
    }
}