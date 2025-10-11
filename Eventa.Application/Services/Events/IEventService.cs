using Eventa.Application.DTOs.Events;
using FluentResults;

namespace Eventa.Application.Services.Events
{
    public interface IEventService
    {
        Task<Result<int>> CreateEventAsync(CreateEventDto dto);
    }
}