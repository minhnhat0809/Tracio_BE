using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;

namespace ContentService.Infrastructure.Repositories;

public class BlogRepo(TracioContentDbContext context) : RepositoryBase<Blog>(context), IBlogRepo
{
    private readonly TracioContentDbContext _context = context;

    public async Task<bool> DeleteBlog(int blogId)
    {
        try
        {
            var blog = await GetByIdAsync(b => b.BlogId == blogId, b => b);
            blog!.Status = "Deleted";
            
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}