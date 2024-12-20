using System.Linq.Expressions;
using ContentService.Application.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ContentService.Infrastructure.Repositories;

public class RepositoryBase<T>(IMongoDatabase database, string collectionName) : IRepositoryBase<T>
    where T : class
{
    private readonly IMongoCollection<T> _collection = database.GetCollection<T>(collectionName);

    public async Task<T?> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> filter)
    {
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<List<T>> FindAsyncWithPagingAndSorting(
        Expression<Func<T, bool>> filter, 
        int pageIndex, 
        int pageSize,
        Expression<Func<T, object>>? sortBy = null,
        bool ascending = true)
    {
        var skip = (pageIndex - 1) * pageSize;

        var query = _collection.Find(filter);

        if (sortBy != null)
        {
            query = ascending
                ? query.SortBy(sortBy)
                : query.SortByDescending(sortBy);
        }

        query = query.Skip(skip).Limit(pageSize);

        return await query.ToListAsync();
    }

    public async Task InsertAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(string id, T entity)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        await _collection.ReplaceOneAsync(filter, entity);
    }

    public async Task DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        await _collection.DeleteOneAsync(filter);
    }
}