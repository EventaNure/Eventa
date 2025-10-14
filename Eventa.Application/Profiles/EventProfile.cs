using System.Xml;
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
                        .Select(tagId => new EventTag
                        {
                            TagId = tagId
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

            CreateMap<UpdateEventDto, Event>()
                .ForMember(
                    e => e.EventTags,
                    opt => opt.MapFrom(dto => dto.TagIds
                        .Select(tagId => new EventTag
                        {
                            TagId = tagId
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
                )
                .ForMember(
                    e => e.Id,
                    opt => opt.MapFrom(dto => dto.EventId)
                );
        }
    }
}
