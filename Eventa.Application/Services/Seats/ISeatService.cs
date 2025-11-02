using Eventa.Application.DTOs.Seats;
using Eventa.Application.DTOs.Sections;
using FluentResults;

namespace Eventa.Application.Services.Sections
{
    public interface ISeatService
    {
        Task<Result<FreeSeatsWithHallPlan>> GetFreeSeatsWithHallPlan(int eventId, string? userId);
    }
}