using System.Linq.Expressions;
using MongoDB.Driver;
using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.Repositories;

public class RepositoryBase<T>(IMongoDatabase database, string collectionName) : IRepositoryBase<T>
    where T : class
{
    private readonly IMongoCollection<T> _collection = database.GetCollection<T>(collectionName);

    public async Task<List<TResult>> FindAsyncWithPagingAndSorting<TResult>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, TResult>> selector,
        int pageIndex,
        int pageSize,
        Expression<Func<T, object>>? sortBy = null,
        bool ascending = true)
    {
        var query = _collection.AsQueryable().Where(filter);

        // Apply sorting if provided
        if (sortBy != null)
        {
            query = ascending ? query.OrderBy(sortBy) : query.OrderByDescending(sortBy);
        }

        // Apply pagination
        query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

        return await Task.FromResult(query.Select(selector).ToList());
    }
    
    public async Task<bool> CreateAsync(T entity)
    {
        try
        {
            await _collection.InsertOneAsync(entity);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MongoDB] ERROR inserting entity: {ex.Message}");
            return false;
        }
    }

    
    public async Task<bool> DeleteAsync(int id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id); 
        var result = await _collection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }
    
    public async Task<long> CountAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.CountDocumentsAsync(filter);
    }
}