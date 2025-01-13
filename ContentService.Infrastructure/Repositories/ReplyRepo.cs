using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;

namespace ContentService.Infrastructure.Repositories;

public class ReplyRepo(TracioContentDbContext context) : RepositoryBase<Reply>(context),IReplyRepo
{
    
}