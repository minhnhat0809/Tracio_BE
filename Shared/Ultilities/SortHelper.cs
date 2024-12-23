using System.Linq.Expressions;
using System.Reflection;

namespace Shared.Ultilities;

public static class SortHelper
{
    public static Expression<Func<T, object>> BuildSortExpression<T>(string sortField)
    {
        if (string.IsNullOrWhiteSpace(sortField))
            throw new ArgumentNullException(nameof(sortField), "Sort field cannot be null or empty.");

        // Compare to property name
        var propertyInfo = typeof(T).GetProperty(
            sortField,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
        );

        if (propertyInfo == null)
        {
            throw new ArgumentException($"Invalid sort field: {sortField}");
        }

        // Create a parameter expression for the entity type
        var parameter = Expression.Parameter(typeof(T), "x");

        // Access the property dynamically
        var propertyAccess = Expression.Property(parameter, propertyInfo);

        // Cast to object (boxing)
        var converted = Expression.Convert(propertyAccess, typeof(object));

        // Return the lambda expression
        return Expression.Lambda<Func<T, object>>(converted, parameter);
    }
}