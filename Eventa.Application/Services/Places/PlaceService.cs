using AutoMapper;
using Eventa.Application.DTOs.Places;
using Eventa.Application.Repositories;
using Eventa.Domain;

namespace Eventa.Application.Services.Places
{
    public class PlaceService : IPlaceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PlaceService(IUnitOfWork unitOfWork, IMapper mapper) {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PlaceDto>> GetPlacesAsync()
        {
            var placeDbSet = _unitOfWork.GetDbSet<Place>();

            var places = await placeDbSet.GetAllAsync();

            return _mapper.Map<IEnumerable<PlaceDto>>(places);
        }
    }
}
