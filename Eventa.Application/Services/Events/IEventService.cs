using Eventa.Application.DTOs.Events;
using FluentResults;

namespace Eventa.Application.Services.Events
{
    public interface IEventService
    {
        Task<Result<int>> CreateEventAsync(CreateEventDto dto);
        Task<Result> DeleteEventAsync(int eventId, string organizerId);
        Task<List<EventListItemDto>> GetEventsAsync(int pageNumber, int pageSize);
        Task<List<EventListItemDto>> GetEventsByTagsAsync(int pageNumber, int pageSize, List<int> tagIds);
        Task<Result> UpdateEventAsync(UpdateEventDto dto);
    }
}