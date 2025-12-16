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

        private const int minimumTagsNumber = 3;

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

            var checkImageSizeResult = CheckImageSize(dto.ImageBytes);
            if (!checkImageSizeResult.IsSuccess)
            {
                return checkImageSizeResult;
            }

            var checkTagsNumberResult = CheckTagsNumber(dto.TagIds);
            if (!checkTagsNumberResult.IsSuccess)
            {
                return checkTagsNumberResult;
            }

            var checkDateTimesResult = CheckDateTimes(dto.DateTimes);
            if (!checkDateTimesResult.IsSuccess)
            {
                return checkDateTimesResult;
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
            eventEntity.ApplicationUserId = dto.OrganizerId;

            var eventDbSet = _unitOfWork.GetDbSet<Event>();
            eventDbSet.Add(eventEntity);
            await _unitOfWork.CommitAsync();


            string fileName = eventEntity.Id + Path.GetExtension(dto.ImageFileName);
            var path = Path.Combine("events", fileName);
            bool isSaved = await _fileService.SaveFile(dto.ImageBytes, path);

            if (!isSaved)
            {
                eventDbSet.Remove(eventEntity);
                await _unitOfWork.CommitAsync();

                return Result.Fail(new Error("Invalid image").WithMetadata("Code", "InvalidImage"));
            }

            return eventEntity.Id;
        }

        public async Task<Result<string>> LoadImageAsync(Stream stream, string fileName)
        {
            var fileNameForSave = Guid.NewGuid() + Path.GetExtension(fileName);
            var path = Path.Combine("preview-events", fileNameForSave);
            await _fileService.SaveFile(stream, path);
            var url = _fileService.GetFileUrlWithSpecificExtension(path)!;
            return Result.Ok(url);
        }

        public async Task<Result> UpdateEventAsync(UpdateEventDto dto)
        {
            if (!_fileService.IsValidExtension(dto.ImageFileName))
            {
                return Result.Fail(new Error("Invalid extension").WithMetadata("Code", "InvalidExtension"));
            }

            var checkImageSizeResult = CheckImageSize(dto.ImageBytes);
            if (!checkImageSizeResult.IsSuccess)
            {
                return checkImageSizeResult;
            }

            var checkTagsNumber = CheckTagsNumber(dto.TagIds);
            if (!checkTagsNumber.IsSuccess)
            {
                return checkTagsNumber;
            }

            var checkDateTimes = CheckDateTimes(dto.DateTimes);
            if (!checkDateTimes.IsSuccess)
            {
                return checkDateTimes;
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

            if (eventEntity.ApplicationUserId != dto.OrganizerId)
            {
                return Result.Fail(new Error("Event not owned by organizer").WithMetadata("Code", "EventNotOwnedByOrganizer"));
            }

            _mapper.Map(dto, eventEntity);
            eventEntity.ApplicationUserId = dto.OrganizerId;
            await _unitOfWork.CommitAsync();

            string fileName = eventEntity.Id + Path.GetExtension(dto.ImageFileName);
            var path = Path.Combine("events", fileName);
            await _fileService.SaveFile(dto.ImageBytes, path);

            return Result.Ok();
        }

        private static Result CheckTagsNumber(IEnumerable<int> tagIds)
        {
            if (tagIds.Count() < minimumTagsNumber)
            {
                return Result.Fail(new Error("Minimum number of tags: " + minimumTagsNumber).WithMetadata("Code", "SmallNumberOfTags"));
            }

            return Result.Ok();
        }

        private Result CheckImageSize(Stream bytes)
        {
            var checkImageSizeResult = _fileService.IsValidSize(bytes);
            if (!checkImageSizeResult)
            {
                return Result.Fail(new Error("Image size too large").WithMetadata("Code", "LargeImageSize"));
            }

            return Result.Ok();
        }

        private static Result CheckDateTimes(IEnumerable<DateTime> dateTimes)
        {
            if (!dateTimes.Any())
            {
                return Result.Fail(new Error("At least one date time must exist").WithMetadata("Code", "DateTimeNotExist"));
            }

            foreach (var date in dateTimes)
            {
                if (date < DateTime.Now)
                {
                    return Result.Fail(new Error("At least one date time in the past").WithMetadata("Code", "DateTimeMustBeInTheFuture"));
                }
            }

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

            if (eventEntity.ApplicationUserId != organizerId)
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

        public async Task<List<EventListItemDto>> GetEventsAsync(int pageNumber, int pageSize, IEnumerable<int> tagIds, DateOnly? startDate, DateOnly? endDate, string? subName)
        {
            var eventRepository = _unitOfWork.GetEventRepository();
            var events = await eventRepository.GetEventsAsync(pageNumber, pageSize, tagIds, startDate, endDate, subName);
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

        public async Task<Result<List<PendingEventListItem>>> GetPendingEventsAsync(int pageNumber, int pageSize)
        {
            var eventRepository = _unitOfWork.GetEventRepository();

            var events = await eventRepository.GetPendingEventsAsync(pageNumber, pageSize);

            foreach (var eventDto in events)
            {
                eventDto.ImageUrl = AddImageUrl(eventDto.Id);
            }

            return Result.Ok(events);
        }

        public async Task<Result> ApproveEventAsync(int eventId)
        {
            var eventDbSet = _unitOfWork.GetDbSet<Event>();

            var eventEntity = await eventDbSet.GetAsync(eventId);

            if (eventEntity == null)
            {
                return Result.Fail(new Error("Event not found").WithMetadata("Code", "EventNotFound"));
            }

            if (eventEntity.EventStatus != EventStatus.Pending)
            {
                return Result.Fail(new Error("Event is not pending").WithMetadata("Code", "EventNotPending"));
            }

            eventEntity.EventStatus = EventStatus.Approved;
            await _unitOfWork.CommitAsync();

            return Result.Ok();
        }

        public async Task<Result> DenyEventAsync(int eventId)
        {
            var eventDbSet = _unitOfWork.GetDbSet<Event>();

            var eventEntity = await eventDbSet.GetAsync(eventId);

            if (eventEntity == null)
            {
                return Result.Fail(new Error("Event not found").WithMetadata("Code", "EventNotFound"));
            }

            if (eventEntity.EventStatus != EventStatus.Pending)
            {
                return Result.Fail(new Error("Event is not pending").WithMetadata("Code", "EventNotPending"));
            }

            eventEntity.EventStatus = EventStatus.Denied;
            await _unitOfWork.CommitAsync();

            return Result.Ok();
        }

        private string? AddImageUrl(int id)
        {
            var path = Path.Combine("events", id.ToString());
            return _fileService.GetFileUrl(path);
        }
    }
}
