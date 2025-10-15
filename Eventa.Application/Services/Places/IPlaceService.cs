using Eventa.Application.DTOs.Places;

namespace Eventa.Application.Services.Places
{
    public interface IPlaceService
    {
        Task<IEnumerable<PlaceDto>> GetPlacesAsync();
    }
}