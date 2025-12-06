using AutoMapper;
using Eventa.Application.DTOs.Comments;
using Eventa.Application.Repositories;
using Eventa.Domain;
using FluentResults;

namespace Eventa.Application.Services.Comments
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CommentService(IUnitOfWork unitOfWork, IMapper mapper) {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<CommentDto>> CreateCommentAsync(string userId, CreateCommentDto dto)
        {
            var orderDbSet = _unitOfWork.GetOrderRepository();
            var order = await orderDbSet.GetOrderWithCommentAsync(dto.OrderId);
            if (order == null)
            {
                return Result.Fail<CommentDto>("Order not found.");
            }
            if (order.UserId != userId)
            {
                return Result.Fail<CommentDto>("User not owned this order.");
            }
            if (order.Comment != null)
            {
                return Result.Fail<CommentDto>("Comment for this order already exists.");
            }

            var comment = _mapper.Map<Comment>(dto);
            comment.CreatedAt = DateTime.UtcNow;
            var commentDbSet = _unitOfWork.GetDbSet<Comment>();
            commentDbSet.Add(comment);
            await _unitOfWork.CommitAsync();
            var commentDto = _mapper.Map<CommentDto>(comment);
            return Result.Ok(commentDto);
        }
    }
}
