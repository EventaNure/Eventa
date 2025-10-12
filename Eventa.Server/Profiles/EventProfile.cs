using AutoMapper;
using Eventa.Application.DTOs.Events;
using Eventa.Server.ResponseModels;

namespace Eventa.Server.Profiles
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<EventListItemDto, EventListItemResponseModel>();
        }
    }
}
