using System.Collections.Generic;
using AutoMapper;
using Eventa.Application.Services.Places;
using Eventa.Server.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventa.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlacesController : ControllerBase
    {
        private readonly IPlaceService _placeService;
        private readonly IMapper _mapper;

        public PlacesController(IPlaceService placeService, IMapper mapper) {
            _placeService = placeService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetPlaces()
        {
            var places = await _placeService.GetPlacesAsync();
            return Ok(_mapper.Map<IEnumerable<PlaceListItemResponseModel>>(places));
        }
    }
}
