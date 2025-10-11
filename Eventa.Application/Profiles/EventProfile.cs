using AutoMapper;
using Eventa.Application.DTOs.Events;
using Eventa.Domain;

namespace Eventa.Application.Profiles
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<CreateEventDto, Event>()
                .ForMember(
                    e => e.EventTags,
                    opt => opt.MapFrom(dto => dto.TagIds
                        .Select(tagId => new Tag
                        {
                            Id = tagId
                        })
                    )
                )
                .ForMember(
                    e => e.EventDateTimes,
                    opt => opt.MapFrom(dto => dto.DateTimes
                        .Select(dateTime => new EventDateTime
                        {
                            StartDateTime = dateTime
                        })
                    )
                );
        }
    }
}
