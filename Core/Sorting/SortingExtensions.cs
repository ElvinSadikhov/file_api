using System.Linq.Expressions;
using System.Reflection;

namespace Core.Sorting;

// todo: add attribute for entity properties to allow sorting. it should be done for security reasons, as attacker might brute force column names
public static class SortingExtensions
{
    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, SortingQueryParams? sorting)
    {
        if (sorting is null || sorting.Prop is null || sorting.Dir is null)
            return query;

        var prop = typeof(T).GetProperty(sorting.Prop,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (prop is null) return query;

        var param = Expression.Parameter(typeof(T), "x");
        var propAccess = Expression.Property(param, prop);
        var lambda = Expression.Lambda(propAccess, param);

        var methodName = sorting.Dir == SortDirectionEnum.Desc ? "OrderByDescending" : "OrderBy";
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), prop.PropertyType);

        return (IQueryable<T>)method.Invoke(null, [query, lambda])!;
    }
}