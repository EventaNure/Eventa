using AutoMapper;
using Eventa.Application.DTOs.TicketInCarts;

namespace Eventa.Application.Profiles
{
    public class TicketInCartProfile : Profile
    {
        public TicketInCartProfile()
        {
            CreateMap<CartDataDto, CartDto>();
        }
    }
}
