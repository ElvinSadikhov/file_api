namespace Core.Filtering;

using System.Linq.Expressions;
using System.Reflection;

public static class SearchExtensions
{
    public static IQueryable<T> ApplyFilter<T, TFilter>(
        this IQueryable<T> query,
        TFilter? filteringQueryParams
    )
        where TFilter : IFilteringQueryParams
    {
        if (filteringQueryParams is null)
            return query;

        var props = typeof(TFilter).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var entityProps = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? predicate = null;

        foreach (var searchProp in props)
        {
            var value = searchProp.GetValue(filteringQueryParams);
            if (value is null) continue;

            var entityProp = entityProps.FirstOrDefault(p =>
                string.Equals(p.Name, searchProp.Name, StringComparison.OrdinalIgnoreCase));

            if (entityProp is null) continue;

            var left = Expression.Property(parameter, entityProp);
            var right = Expression.Constant(value, entityProp.PropertyType);

            Expression comparison;

            if (entityProp.PropertyType == typeof(string))
            {
                var toLower = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
                var contains = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

                var leftLower = Expression.Call(left, toLower);
                var rightLower = Expression.Constant(value.ToString()!.ToLower());

                comparison = Expression.Call(leftLower, contains, rightLower);
            }
            else if (entityProp.PropertyType == typeof(int) && searchProp.Name.StartsWith("Min"))
            {
                comparison = Expression.GreaterThanOrEqual(left, right);
            }
            else if (entityProp.PropertyType == typeof(int) && searchProp.Name.StartsWith("Max"))
            {
                comparison = Expression.LessThanOrEqual(left, right);
            }
            else
            {
                comparison = Expression.Equal(left, right);
            }

            predicate = predicate is null
                ? comparison
                : Expression.AndAlso(predicate, comparison);
        }

        if (predicate is null) return query;

        var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
        return query.Where(lambda);
    }
}