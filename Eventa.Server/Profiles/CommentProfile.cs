using AutoMapper;
using Eventa.Application.DTOs.Comments;
using Eventa.Domain;
using Eventa.Server.RequestModels;

namespace Eventa.Server.Profiles
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<CreateCommentRequestModel, CreateCommentDto>();
        }
    }
}
