using Eventa.Application.DTOs.Seats;
using Eventa.Application.DTOs.Sections;
using Eventa.Application.Repositories;
using FluentResults;

namespace Eventa.Application.Services.Sections
{
    public class SeatService : ISeatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public SeatService(IUnitOfWork unitOfWork, IFileService fileService) {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        public async Task<Result<FreeSeatsWithHallPlan>> GetFreeSeatsWithHallPlan(int eventDateTimeId, string? userId)
        {
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
