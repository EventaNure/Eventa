using AutoMapper;
using Eventa.Application.DTOs.Events;
using Eventa.Application.Repositories;
using Eventa.Domain;
using FluentResults;

namespace Eventa.Application.Services.Events
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EventService(IUnitOfWork unitOfWork, IMapper mapper) {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Result<int>> CreateEventAsync(CreateEventDto dto)
        {
            var eventDbSet = _unitOfWork.GetDbSet<Event>();
            var eventEntity = _mapper.Map<Event>(dto);
            eventDbSet.Add(eventEntity);

            await _unitOfWork.CommitAsync();
            return eventEntity.Id;
        }

        public async Task<List<EventListItemDto>> GetEventsAsync(int pageNumber, int pageSize)
        {
            var eventRepository = _unitOfWork.GetEventRepository();
            return await eventRepository.GetEventsAsync(pageNumber, pageSize);
        }
        public async Task<List<EventListItemDto>> GetEventsByTagsAsync(int pageNumber, int pageSize, List<int> tagIds)
        {
            var eventRepository = _unitOfWork.GetEventRepository();
            return await eventRepository.GetEventsByTagsAsync(pageNumber, pageSize, tagIds);
        }
    }
}
