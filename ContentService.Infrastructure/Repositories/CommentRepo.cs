using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;

namespace ContentService.Infrastructure.Repositories;

public class CommentRepo(TracioContentDbContext context) : RepositoryBase<Comment>(context),ICommentRepo
{
    
}