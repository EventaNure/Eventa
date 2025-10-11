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
    }
}
