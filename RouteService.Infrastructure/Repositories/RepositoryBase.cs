using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RouteService.Application.Interfaces;
using RouteService.Infrastructure.Contexts;
namespace RouteService.Infrastructure.Repositories;

public class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected readonly TracioRouteDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public RepositoryBase(TracioRouteDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public async Task<(IEnumerable<T> Items, int TotalCount)> GetAllAsync(
        int pageIndex, int pageSize, 
        string? sortBy, bool sortDesc, 
        Dictionary<string, string>? filters = null, 
        string includeProperties = "")
    {
        IQueryable<T> query = _dbSet;
        
        // Apply to Include for Related Tables
        foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        // Apply filtering if provided
        if (filters != null && filters.Any())
        {
            foreach (var filter in filters)
            {
                var propertyType = typeof(T).GetProperty(filter.Key)?.PropertyType;

                if (propertyType == typeof(string))
                {
                    query = query.Where(e => EF.Property<string>(e, filter.Key).Contains(filter.Value));
                }
                else
                {
                    query = query.Where(e => EF.Property<object>(e, filter.Key).ToString() == filter.Value);
                }
            }
        }

        // Get total count after filtering
        int totalCount = await query.CountAsync();

        // Apply sorting if provided
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortDesc
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        }

        // Apply pagination
        query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

        return (await query.ToListAsync(), totalCount);
    }

    public async Task<T?> GetByIdAsync(object id, string includeProperties = "")
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));

        IQueryable<T> query = _dbSet;

        // Include navigation properties
        foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        // Try to use FindAsync if possible (Primary Key Lookup)
        var entity = await _dbSet.FindAsync(id);
    
        // If FindAsync doesn't work, fall back to query filtering
        if (entity == null)
        {
            var primaryKeyName = _context.Model
                .FindEntityType(typeof(T))
                ?.FindPrimaryKey()
                ?.Properties
                .FirstOrDefault()
                ?.Name;

            if (string.IsNullOrEmpty(primaryKeyName))
                throw new InvalidOperationException($"No primary key defined for {typeof(T).Name}");

            entity = await query.FirstOrDefaultAsync(e => EF.Property<object>(e, primaryKeyName).Equals(id));
        }

        return entity;
    }
    
    public async Task<T?> GetByFilterAsync(Dictionary<string, string>? filters, string includeProperties = "")
    {
        IQueryable<T> query = _dbSet;

        // Include related tables
        foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        // Apply filtering if provided
        if (filters != null && filters.Any())
        {
            foreach (var filter in filters)
            {
                var propertyType = typeof(T).GetProperty(filter.Key)?.PropertyType;

                if (propertyType == typeof(string))
                {
                    query = query.Where(e => EF.Property<string>(e, filter.Key).Contains(filter.Value));
                }
                else
                {
                    query = query.Where(e => EF.Property<object>(e, filter.Key).ToString() == filter.Value);
                }
            }
        }

        return await query.FirstOrDefaultAsync();
    }


    public async Task<TResult?> GetById<TResult>(
        Expression<Func<T, bool>> expression,
        Expression<Func<T, TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(selector);

        return await _context.Set<T>()
            .Where(expression)
            .Select(selector)
            .FirstOrDefaultAsync();
    }


    // Create
    public async Task<T?> CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Update
    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    // Hard Delete
    public async Task DeleteAsync(object id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Soft Delete (assumes the entity has a "IsDeleted" property)
    public async Task SoftDeleteAsync(object id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            var isDeletedProperty = entity.GetType().GetProperty("IsDeleted");
            if (isDeletedProperty != null)
            {
                isDeletedProperty.SetValue(entity, true);
                await UpdateAsync(entity);
            }
            else
            {
                throw new InvalidOperationException("Entity does not support soft delete.");
            }
        }
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        var primaryKeyName = _context.Model
            .FindEntityType(typeof(T))
            ?.FindPrimaryKey()
            ?.Properties
            .FirstOrDefault()
            ?.Name;

        if (string.IsNullOrEmpty(primaryKeyName))
            throw new InvalidOperationException($"No primary key defined for {typeof(T).Name}");

        return await _dbSet.AnyAsync(e => EF.Property<int>(e, primaryKeyName) == id);
    }
}
