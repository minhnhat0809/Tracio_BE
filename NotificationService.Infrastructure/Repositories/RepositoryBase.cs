using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
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
    
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.Find(filter).AnyAsync();
    }
    
    public async Task<bool> UpdateFieldsAsync(
        Expression<Func<T, bool>> filter,
        params (Expression<Func<T, object>> field, object value)[] updates)
    {
        var updateDef = new List<UpdateDefinition<T>>();
        foreach (var (field, value) in updates)
        {
            updateDef.Add(Builders<T>.Update.Set(field, value));
        }

        var update = Builders<T>.Update.Combine(updateDef);
        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }
    
    public async Task<TResult?> GetByIdAsync<TResult>(
        Expression<Func<T, bool>> expression,
        Expression<Func<T, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(selector);

        return await _collection.AsQueryable()
            .Where(expression)
            .Select(selector)
            .FirstOrDefaultAsync();
    }

    public async Task<List<TResult>> FindAsync<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> selector)
    {
        var query = _collection.Find(filter);

        return await query.Project(selector).ToListAsync();
    }
}