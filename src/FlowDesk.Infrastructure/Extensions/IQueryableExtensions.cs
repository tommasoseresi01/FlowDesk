using System.Linq.Expressions;
using System.Reflection;
using FlowDesk.Application.Models.Common;

namespace FlowDesk.Infrastructure.Extensions;

public static class IQueryableExtensions
{
    // Ordina per il nome di una colonna, anche con percorso puntato ("UserModification.Email").
    // Restituisce null se la colonna non esiste o non è ordinabile: il chiamante usa il suo ordine di default.
    public static IOrderedQueryable<T>? OrderByColumn<T>(this IQueryable<T> source, SortingInfo? sortingInfo)
    {
        if (sortingInfo is null || string.IsNullOrWhiteSpace(sortingInfo.ColumnName))
        {
            return null;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression body = parameter;
        foreach (var part in sortingInfo.ColumnName.Split('.'))
        {
            var property = body.Type.GetProperty(
                part,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property is null)
            {
                return null;
            }

            body = Expression.Property(body, property);
        }

        // Si ordina solo per valori semplici, mai per collezioni o oggetti.
        if (!IsSortable(body.Type))
        {
            return null;
        }

        var methodName = sortingInfo.SortDirection == SortDirectionEnum.DESCENDING
            ? nameof(Queryable.OrderByDescending)
            : nameof(Queryable.OrderBy);

        var call = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), body.Type],
            source.Expression,
            Expression.Quote(Expression.Lambda(body, parameter)));

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
    }

    private static bool IsSortable(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.IsPrimitive
            || underlying.IsEnum
            || underlying == typeof(string)
            || underlying == typeof(decimal)
            || underlying == typeof(DateTime)
            || underlying == typeof(Guid);
    }
}
