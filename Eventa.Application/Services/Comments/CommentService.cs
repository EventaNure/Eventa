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
        private readonly IUserService _userService;

        public CommentService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService) {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<Result<CommentDto>> CreateCommentAsync(string userId, CreateCommentDto dto)
        {
            var orderDbSet = _unitOfWork.GetOrderRepository();
            var order = await orderDbSet.GetOrderWithCommentAsync(dto.OrderId);
            if (order == null)
            {
                return Result.Fail(new Error("Order not found.")
                    .WithMetadata("Code", "OrderNotFound"));
            }

            if (order.UserId != userId)
            {
                return Result.Fail(new Error("User does not own this order.")
                    .WithMetadata("Code", "UserNotOwner"));
            }

            if (order.Comment != null)
            {
                return Result.Fail(new Error("Comment for this order already exists.")
                    .WithMetadata("Code", "CommentAlreadyExists"));
            }

            if (!order.IsQrTokenUsed)
            {
                return Result.Fail(new Error("Qr-Token is not used yet.")
                    .WithMetadata("Code", "QrTokenIsNotUsed"));
            }

            var comment = _mapper.Map<Comment>(dto);
            comment.CreatedAt = DateTime.UtcNow;
            comment.UserId = userId;

            var commentDbSet = _unitOfWork.GetDbSet<Comment>();
            commentDbSet.Add(comment);
            await _unitOfWork.CommitAsync();

            var commentDto = _mapper.Map<CommentDto>(comment);

            var getUserNameResult = await _userService.GetUserNameAsync(userId);
            if (!getUserNameResult.IsSuccess)
            {
                return Result.Fail(getUserNameResult.Errors);
            }
            commentDto.UserName = getUserNameResult.Value;

            return Result.Ok(commentDto);
        }
    }
}
