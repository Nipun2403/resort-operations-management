using System.Linq.Expressions;
using System.Reflection;

namespace HotelManagement.Repository.Utilities;

public static class QueryableExtensions
{
    public static IOrderedQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string propertyName, bool descending)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or empty.");

        var parameter = Expression.Parameter(typeof(T), "x");
        
        // Find property case-insensitively
        var property = typeof(T).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                                .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

        if (property == null)
            throw new ArgumentException($"Property '{propertyName}' does not exist on type '{typeof(T).Name}'.");

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";
        
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), property.PropertyType },
            source.Expression,
            Expression.Quote(orderByExpression)
        );

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(resultExpression);
    }

    public static IOrderedQueryable<T> ThenByDynamic<T>(this IOrderedQueryable<T> source, string propertyName, bool descending)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or empty.");

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = typeof(T).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                                .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

        if (property == null)
            throw new ArgumentException($"Property '{propertyName}' does not exist on type '{typeof(T).Name}'.");

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = descending ? "ThenByDescending" : "ThenBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), property.PropertyType },
            source.Expression,
            Expression.Quote(orderByExpression)
        );

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(resultExpression);
    }
}
