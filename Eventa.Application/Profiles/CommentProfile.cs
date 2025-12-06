using AutoMapper;
using Eventa.Application.DTOs.Comments;
using Eventa.Domain;

namespace Eventa.Application.Profiles
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<CreateCommentDto, Comment>();
            CreateMap<Comment, CommentDto>();
        }
    }
}
