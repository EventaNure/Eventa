using Eventa.Application.DTOs.Comments;
using FluentResults;

namespace Eventa.Application.Services.Comments
{
    public interface ICommentService
    {
        Task<Result<CommentDto>> CreateCommentAsync(string userId, CreateCommentDto dto);
    }
}