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
            if (!_fileService.IsValidExtension(dto.ImageFileName))
            {
                return Result.Fail(new Error("Invalid extension").WithMetadata("Code", "InvalidExtension"));
            }

            var checkTagExistingResult = await CheckTagsExistingAsync(dto.TagIds);
            if (!checkTagExistingResult.IsSuccess)
            {
                return checkTagExistingResult;
            }

            var checkPlaceExistingResult = await CheckPlaceExistingAsync(dto.PlaceId);
            if (!checkPlaceExistingResult.IsSuccess)
            {
                return checkPlaceExistingResult;
            }

            var eventEntity = _mapper.Map<Event>(dto);

            var eventDbSet = _unitOfWork.GetDbSet<Event>();
            eventDbSet.Add(eventEntity);
            await _unitOfWork.CommitAsync();


            string fileName = eventEntity.Id + Path.GetExtension(dto.ImageFileName);

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
            if (!_fileService.IsValidExtension(dto.ImageFileName))
            {
                return Result.Fail(new Error("Invalid extension").WithMetadata("Code", "InvalidExtension"));
            }

            var checkTagExistingResult = await CheckTagsExistingAsync(dto.TagIds);
            if (!checkTagExistingResult.IsSuccess)
            {
                return checkTagExistingResult;
            }

            var checkPlaceExistingResult = await CheckPlaceExistingAsync(dto.PlaceId);
            if (!checkPlaceExistingResult.IsSuccess)
            {
                return checkPlaceExistingResult;
            }

            var eventRepository = _unitOfWork.GetEventRepository();
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

            string fileName = eventEntity.Id + Path.GetExtension(dto.ImageFileName);

            await _fileService.UpdateFile(dto.ImageBytes, fileName);

            return Result.Ok();
        }

        private async Task<Result> CheckTagsExistingAsync(IEnumerable<int> tagIds)
        {
            var tagRepository = _unitOfWork.GetTagRepository();
            var tags = await tagRepository.GetByIdsAsync(tagIds);
            if (tags.Count() != tagIds.Count())
            {
                return Result.Fail(new Error("At least one tag doesn't exist").WithMetadata("Code", "TagNotExist"));
            }

            return Result.Ok();
        }

        private async Task<Result> CheckPlaceExistingAsync(int placeId)
        {
            var placeDbSet = _unitOfWork.GetDbSet<Place>();
            var placeEntity = await placeDbSet.GetAsync(placeId);
            if (placeEntity == null)
            {
                return Result.Fail(new Error("Place doesn't exist").WithMetadata("Code", "PlaceNotExist"));
            }

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

        public async Task<Result<EventDto>> GetEventAsync(int eventId)
        {
            var eventRepository = _unitOfWork.GetEventRepository();
            var eventDto = await eventRepository.GetEventAsync(eventId);
            if (eventDto == null)
            {
                return Result.Fail(new Error("Event not found").WithMetadata("Code", "EventNotFound"));
            }
            eventDto.ImageUrl = AddImageUrl(eventId);

            return eventDto;
        }

        public async Task<List<EventListItemDto>> GetEventsAsync(int pageNumber, int pageSize, IEnumerable<int> tagIds)
        {
            var eventRepository = _unitOfWork.GetEventRepository();
            var events = await eventRepository.GetEventsAsync(pageNumber, pageSize, tagIds);
            foreach (var eventDto in events)
            {
                eventDto.ImageUrl = AddImageUrl(eventDto.Id);
            }

            return events;
        }

        public async Task<List<EventListItemDto>> GetEventsByOrganizerAsync(int pageNumber, int pageSize, string organizerId)
        {
            var eventRepository = _unitOfWork.GetEventRepository();

            var events = await eventRepository.GetEventsByOrganizerAsync(pageNumber, pageSize, organizerId);

            foreach (var eventDto in events)
            {
                eventDto.ImageUrl = AddImageUrl(eventDto.Id);
            }

            return events;
        }

        private string? AddImageUrl(int id)
        {
            var path = Path.Combine("events", $"{id}.jpg");
            return _fileService.GetFileUrl(path);
        }
    }
}
