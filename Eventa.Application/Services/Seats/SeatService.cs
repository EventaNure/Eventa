using Eventa.Application.DTOs.Seats;
using Eventa.Application.Repositories;
using FluentResults;

namespace Eventa.Application.Services.Sections
{
    public class SeatService : ISeatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IUserService _userService;

        public SeatService(IUnitOfWork unitOfWork, IFileService fileService, IUserService userService) {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _userService = userService;
        }

        public async Task<Result<FreeSeatsWithHallPlan>> GetFreeSeatsWithHallPlan(int eventDateTimeId, string? userId)
        {
            if (userId != null)
            {
                var cartRepository = _unitOfWork.GetCartRepository();
                await cartRepository.DeleteTicketsForOtherEventDateTimeAsync(userId, eventDateTimeId);
                await _userService.DeleteInformationAboutCartAsync(userId);
            }
            var sectionRepository = _unitOfWork.GetSeatRepository();

            var getFreeSeatsResultDto = await sectionRepository.GetFreeSeatsAsync(eventDateTimeId, userId);

            if (getFreeSeatsResultDto == null)
            {
                return Result.Fail(new Error("Event date time not found").WithMetadata("Code", "EventDateTimeNotFound"));
            }

            var hallPlanUrl = _fileService.GetFileUrl($"places/{getFreeSeatsResultDto.PlaceId}");

            if (hallPlanUrl == null)
            {
                return Result.Fail(new Error("Hall plan not found").WithMetadata("Code", "HallPlanNotFound"));
            }

            var dto = new FreeSeatsWithHallPlan
            {
                HallPlanUrl = hallPlanUrl,
                RowTypes = getFreeSeatsResultDto.RowTypes
            };
            

            return Result.Ok(dto);
        }
    }
}
