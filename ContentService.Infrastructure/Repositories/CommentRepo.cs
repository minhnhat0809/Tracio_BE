using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;

namespace ContentService.Infrastructure.Repositories;

public class CommentRepo(TracioContentDbContext context) : RepositoryBase<Comment>(context),ICommentRepo
{
    
}