using ContentService.Domain.Entities;

namespace ContentService.Application.Interfaces;

public interface IReplyRepo : IRepositoryBase<Reply>
{
    Task<(int CommentId, int ReplyIndex)> GetReplyIndex(int replyId);
}