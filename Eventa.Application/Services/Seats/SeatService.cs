﻿using Eventa.Application.DTOs.Seats;
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

        public async Task<Result<FreeSeatsWithHallPlan>> GetFreeSeatsWithHallPlan(int eventId)
        {
            var sectionRepository = _unitOfWork.GetSectionRepository();

            var getFreeSeatsResultDto = await sectionRepository.GetFreeSeatsAsync(eventId);

            if (getFreeSeatsResultDto == null)
            {
                return Result.Fail(new Error("Event not found").WithMetadata("Code", "EventNotFound"));
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
