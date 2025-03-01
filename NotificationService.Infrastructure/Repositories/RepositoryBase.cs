using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Bson;
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
        var query = _collection.Find(filter);

        // Apply sorting if provided
        if (sortBy != null)
        {
            query = ascending ? query.SortBy(sortBy) : query.SortByDescending(sortBy);
        }

        // Apply pagination
        query = query.Skip((pageIndex - 1) * pageSize).Limit(pageSize);

        return await query.Project(selector).ToListAsync();
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

    public async Task<bool> DeleteAsync(string id) // Use string ID
    {
        var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id)); // Ensure ObjectId
        var result = await _collection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.CountDocumentsAsync(filter);
    }
}