﻿using Eventa.Application.DTOs.Events;
using Eventa.Domain;

namespace Eventa.Application.Repositories
{
    public interface IEventRepository
    {
        Task<EventDto?> GetEventAsync(int id);
        Task<Event?> GetByIdAsync(int id);
        Task<List<EventListItemDto>> GetEventsAsync(int pageNumber, int pageSize, IEnumerable<int> tagIds);
        Task<List<EventListItemDto>> GetEventsByOrganizerAsync(int pageNumber, int pageSize, string organizerId);
    }
}