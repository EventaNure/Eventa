using Eventa.Application.DTOs.Events;
using Eventa.Domain;

namespace Eventa.Application.Repositories
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(int id);
        Task<List<EventListItemDto>> GetEventsAsync(int pageNumber, int pageSize);
        Task<List<EventListItemDto>> GetEventsByTagsAsync(int pageNumber, int pageSize, List<int> tagIds);
    }
}