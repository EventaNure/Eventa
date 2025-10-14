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
        private readonly IFileService _fileService;

        public EventService(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService) {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileService = fileService;
        }
        public async Task<Result<int>> CreateEventAsync(CreateEventDto dto)
        {
            string extension = Path.GetExtension(dto.ImageFileName);

            if (extension != ".jpg")
            {
                return Result.Fail(new Error("Invalid extension").WithMetadata("Code", "InvalidExtension"));
            }

            var eventDbSet = _unitOfWork.GetDbSet<Event>();
            var tagRepository = _unitOfWork.GetTagRepository();
            var tags = await tagRepository.GetByIdsAsync(dto.TagIds);
            if (tags.Count() != dto.TagIds.Count())
            {
                return Result.Fail(new Error("At least one tag doesn't exist").WithMetadata("Code", "TagNotExist"));
            }
            var eventEntity = _mapper.Map<Event>(dto);
            eventDbSet.Add(eventEntity);
            await _unitOfWork.CommitAsync();


            string fileName = eventEntity.Id + extension;

            bool isSaved = !_fileService.Exists(fileName) && await _fileService.SaveFile(dto.ImageBytes, fileName);

            if (!isSaved)
            {
                eventDbSet.Remove(eventEntity);
                await _unitOfWork.CommitAsync();

                return Result.Fail(new Error("Invalid image").WithMetadata("Code", "InvalidImage"));
            }

            return eventEntity.Id;
        }

        public async Task<Result> UpdateEventAsync(UpdateEventDto dto)
        {
            string extension = Path.GetExtension(dto.ImageFileName);

            if (extension != ".jpg")
            {
                return Result.Fail(new Error("Invalid extension").WithMetadata("Code", "InvalidExtension"));
            }

            var eventRepository = _unitOfWork.GetEventRepository();
            var tagRepository = _unitOfWork.GetTagRepository();
            var tags = await tagRepository.GetByIdsAsync(dto.TagIds);
            if (tags.Count() != dto.TagIds.Count())
            {
                return Result.Fail(new Error("At least one tag doesn't exist").WithMetadata("Code", "TagNotExist"));
            }

            var eventEntity = await eventRepository.GetByIdAsync(dto.EventId);
            if (eventEntity == null)
            {
                return Result.Fail(new Error("Event not found").WithMetadata("Code", "EventNotFound"));
            }

            if (eventEntity.OrganizerId != dto.OrganizerId)
            {
                return Result.Fail(new Error("Event not owned by organizer").WithMetadata("Code", "EventNotOwnedByOrganizer"));
            }

            _mapper.Map(dto, eventEntity);

            await _unitOfWork.CommitAsync();

            string fileName = eventEntity.Id + extension;

            await _fileService.UpdateFile(dto.ImageBytes, fileName);

            return Result.Ok();
        }

        public async Task<Result> DeleteEventAsync(int eventId, string organizerId)
        {
            var eventDbSet = _unitOfWork.GetDbSet<Event>();
            var eventEntity = await eventDbSet.GetAsync(eventId);
            if (eventEntity == null)
            {
                return Result.Fail(new Error("Event not found").WithMetadata("Code", "EventNotFound"));
            }

            if (eventEntity.OrganizerId != organizerId)
            {
                return Result.Fail(new Error("Event not owned by organizer").WithMetadata("Code", "EventNotOwnedByOrganizer"));
            }

            eventDbSet.Remove(eventEntity);

            string fileName = eventId + ".jpg";

            _fileService.DeleteFile(fileName);

            await _unitOfWork.CommitAsync();

            return Result.Ok();
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
