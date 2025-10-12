using Eventa.Application.DTOs.Events;

namespace Eventa.Application.Repositories
{
    public interface IEventRepository
    {
        Task<List<EventListItemDto>> GetEventsAsync(int pageNumber, int pageSize);
        Task<List<EventListItemDto>> GetEventsByTagsAsync(int pageNumber, int pageSize, List<int> tagIds);
    }
}